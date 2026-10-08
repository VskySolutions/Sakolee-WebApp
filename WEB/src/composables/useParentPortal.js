import { computed, ref } from "vue";
import { useAuthStore } from "stores/auth";
import { classApi, classRoomApi, studentApi, getApiErrorMessage } from "services/api";
import { Permissions, usePermissions } from "composables/usePermissions";

// Family-contact logins (Parent/Guardian) get the parent-portal pages. Someone who also holds a staff role
// keeps the studio pages — mirrors the API's IsSelfServiceOnly check.
const PARENT_ROLES = ["Parent", "Guardian"];

export function useIsParentPortal () {
  const auth = useAuthStore();
  return computed(() => auth.roles.length > 0 && auth.roles.every((role) => PARENT_ROLES.includes(role)));
}

// Class.ActiveDays is a comma-separated "Mon, Wed" string and Start/End Time are "hh:mm AM/PM"
// (see ClassFormFields / AppTimeField).
const DAY_KEYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const DAY_NAMES = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

export const toMinutes = (value) => {
  const m = /^(\d{1,2}):(\d{2})\s*(AM|PM)$/i.exec(String(value || "").trim());
  if (!m) return null;
  let hour = Number(m[1]) % 12;
  if (m[3].toUpperCase() === "PM") hour += 12;
  return hour * 60 + Number(m[2]);
};

// "04:30 PM" → "4:30 PM"
export const shortTime = (value) => String(value || "").trim().replace(/^0/, "");

const classDays = (cls) => (cls.activeDays || "").split(",").map((d) => d.trim()).filter((d) => DAY_KEYS.includes(d));

// "Mon & Wed, 4:30–6:00 PM" / "Friday, 5:00 PM" — the prototype's class-card schedule line.
export const formatClassSchedule = (cls) => {
  const days = classDays(cls);
  const dayText = days.length === 1 ? DAY_NAMES[DAY_KEYS.indexOf(days[0])] : days.join(" & ");
  const start = shortTime(cls.startTime);
  const end = shortTime(cls.endTime);
  let timeText = start;
  if (start && end) {
    const [startClock, startMeridiem] = start.split(" ");
    const [endClock, endMeridiem] = end.split(" ");
    timeText = startMeridiem === endMeridiem ? `${startClock}–${endClock} ${endMeridiem}` : `${start}–${end}`;
  }
  return [dayText, timeText].filter(Boolean).join(", ");
};

const startOfDay = (date) => new Date(date.getFullYear(), date.getMonth(), date.getDate());

export const runsOn = (cls, date) => {
  if (!classDays(cls).includes(DAY_KEYS[date.getDay()])) return false;
  const day = startOfDay(date);
  if (cls.startDate && startOfDay(new Date(cls.startDate)) > day) return false;
  if (cls.endDate && startOfDay(new Date(cls.endDate)) < day) return false;
  return true;
};

// The next session of a class from now (today counts until its start time passes), within two weeks.
export const nextSessionOf = (cls, now = new Date()) => {
  const nowMinutes = now.getHours() * 60 + now.getMinutes();
  for (let offset = 0; offset < 14; offset++) {
    const date = new Date(now.getFullYear(), now.getMonth(), now.getDate() + offset);
    if (!runsOn(cls, date)) continue;
    if (offset === 0 && (toMinutes(cls.startTime) ?? 0) < nowMinutes) continue;
    return { date, offset };
  }
  return null;
};

// "Today" / "Tomorrow" / "Wednesday" (this week) / "July 15th"
export const relativeDay = ({ date, offset }, { longDate = false } = {}) => {
  if (offset === 0) return "Today";
  if (offset === 1) return "Tomorrow";
  if (offset < 7 && !longDate) return DAY_NAMES[date.getDay()];
  const day = date.getDate();
  const suffix = day % 10 === 1 && day !== 11 ? "st" : day % 10 === 2 && day !== 12 ? "nd" : day % 10 === 3 && day !== 13 ? "rd" : "th";
  return `${date.toLocaleDateString(undefined, { month: "long" })} ${day}${suffix}`;
};

export const studentFullName = (s) => [s.firstName, s.lastName].filter(Boolean).join(" ") || "Student";

/**
 * The signed-in family contact's students and the classes they are enrolled in, plus best-effort room and
 * instructor names. The API scopes both lists to the contact's own families (see ClassesController /
 * StudentsController). The Parent role's permissions are tuned per tenant, so every source is optional:
 * a call the role can't make is skipped.
 */
export function useParentPortalData () {
  const { has, hasAny } = usePermissions();

  const loading = ref(true);
  const error = ref("");
  const students = ref([]);
  const classes = ref([]);
  const instructorNames = ref({});
  const roomNames = ref({});

  const classesById = computed(() => new Map(classes.value.map((c) => [c.classId, c])));

  // The active classes one student is enrolled in (StudentClasses, falling back to Student.ClassId).
  const classesOf = (student) => {
    const ids = student.classIds?.length ? student.classIds : (student.classId ? [student.classId] : []);
    return ids.map((id) => classesById.value.get(id)).filter((c) => c && c.active);
  };

  const roomOf = (cls) => roomNames.value[cls.roomId] || "";
  const instructorOf = (cls) => instructorNames.value[cls.primaryInstructorId] || "";

  const load = async () => {
    loading.value = true;
    error.value = "";
    try {
      const canListClasses = hasAny([Permissions.ClassesRead, Permissions.StudentsRead, Permissions.StudentsWrite, Permissions.FamiliesWrite]);
      const [studentRes, classRes, instructorRes, roomRes] = await Promise.allSettled([
        has(Permissions.StudentsRead) ? studentApi.list({ limit: 100 }) : Promise.resolve(null),
        canListClasses ? classApi.list({ limit: 100, active: true }) : Promise.resolve(null),
        has(Permissions.ClassesRead) ? classApi.instructors() : Promise.resolve(null),
        has(Permissions.ClassRoomsRead) ? classRoomApi.list({ limit: 100 }) : Promise.resolve(null)
      ]);

      const failed = [studentRes, classRes].find((r) => r.status === "rejected");
      if (failed) error.value = getApiErrorMessage(failed.reason, "Couldn't load your students.");

      students.value = studentRes.value?.data || [];
      classes.value = classRes.value?.data || [];
      instructorNames.value = Object.fromEntries((instructorRes.value?.data || []).map((i) => [i.id, i.name]));
      roomNames.value = Object.fromEntries((roomRes.value?.data || []).map((r) => [r.id, r.name]));
    } finally {
      loading.value = false;
    }
  };

  return { loading, error, students, classes, classesOf, roomOf, instructorOf, load };
}
