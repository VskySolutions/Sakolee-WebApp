// Start/End Time are "hh:mm AM/PM" strings (see AppTimeField); this turns one into minutes-since-
// midnight so Duration can be derived from the pair instead of typed separately.
const timeToMinutes = (display) => {
  const m = /^(\d{2}):(\d{2}) (AM|PM)$/.exec(String(display || "").trim());
  if (!m) return null;
  let hour = Number(m[1]);
  const minute = Number(m[2]);
  if (hour < 1 || hour > 12 || minute > 59) return null;
  if (m[3] === "AM") hour = hour === 12 ? 0 : hour;
  else hour = hour === 12 ? 12 : hour + 12;
  return hour * 60 + minute;
};

// Duration is always derived from Start/End Time rather than typed separately, so it can never
// disagree with the times it describes. Empty until both times are fully entered.
export const formatDuration = (startTime, endTime) => {
  const start = timeToMinutes(startTime);
  const end = timeToMinutes(endTime);
  if (start === null || end === null) return "";
  let minutes = end - start;
  if (minutes < 0) minutes += 24 * 60; // class runs past midnight
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  const hourPart = hours > 0 ? `${hours} hour${hours === 1 ? "" : "s"}` : "";
  const minutePart = `${String(mins).padStart(2, "0")} minute${mins === 1 ? "" : "s"}`;
  return hourPart ? `${hourPart} ${minutePart}` : minutePart;
};

// A blank Class create/edit form — one canonical shape reused by the Add/Edit/View class pages.
// Location, Room, Session and Instructor are still placeholder-only fields (see ClassFormFields.vue)
// — none of them are sent on submit, since the Class record has no backing columns for them yet.
// Category1/2/3 are backed by real Class.Category1Id/2Id/3Id columns and are sent on submit.
export const blankClassForm = () => ({
  classId: null,
  className: "",
  category1: "",
  category2: "",
  category3: "",
  location: "",
  room: "",
  session: "",
  primaryInstructor: "",
  additionalInstructors: "",
  gender: "",
  minAge: null,
  maxAge: null,
  description: "",
  startDate: "",
  endDate: "",
  registrationOpenDate: "",
  cutoffDate: "",
  activeDays: "",
  duration: "",
  startTime: "",
  endTime: "",
  tuitionFee: null,
  billingMethod: "",
  billingCycle: "",
  registrationFee: false,
  dropInFee: false,
  maxClassSize: null,
  maxWaitlistSize: null,
  allowWaitlistInRoll: false,
  allowWaitlistEnrollment: false,
  allowDropIns: false,
  makeupsInClass: false,
  virtualClassUrl: "",
  linkDisplayText: "",
  onlineListings: false,
  onlineRegistration: false,
  allowPortalEnrollment: false,
  parentPortalSchedule: false,
  allowPortalDropRequests: false,
  policyGroups: "",
  active: true
});

// The class payload sent to classApi.create/update — everything in blankClassForm() except the
// placeholder-only fields and (for create) `active`, which the server always defaults to true.
export const toClassPayload = (form) => ({
  className: form.className,
  category1Id: form.category1 || null,
  category2Id: form.category2 || null,
  category3Id: form.category3 || null,
  additionalInstructors: form.additionalInstructors || null,
  gender: form.gender || null,
  minAge: form.minAge,
  maxAge: form.maxAge,
  description: form.description || null,
  startDate: form.startDate || null,
  endDate: form.endDate || null,
  registrationOpenDate: form.registrationOpenDate || null,
  cutoffDate: form.cutoffDate || null,
  activeDays: form.activeDays || null,
  duration: form.duration || null,
  startTime: form.startTime || null,
  endTime: form.endTime || null,
  tuitionFee: form.tuitionFee,
  billingMethod: form.billingMethod || null,
  billingCycle: form.billingCycle || null,
  registrationFee: form.registrationFee,
  dropInFee: form.dropInFee,
  maxClassSize: form.maxClassSize,
  maxWaitlistSize: form.maxWaitlistSize,
  allowWaitlistInRoll: form.allowWaitlistInRoll,
  allowWaitlistEnrollment: form.allowWaitlistEnrollment,
  allowDropIns: form.allowDropIns,
  makeupsInClass: form.makeupsInClass,
  virtualClassUrl: form.virtualClassUrl || null,
  linkDisplayText: form.linkDisplayText || null,
  onlineListings: form.onlineListings,
  onlineRegistration: form.onlineRegistration,
  allowPortalEnrollment: form.allowPortalEnrollment,
  parentPortalSchedule: form.parentPortalSchedule,
  allowPortalDropRequests: form.allowPortalDropRequests,
  policyGroups: form.policyGroups || null
});

// Maps a ClassSummary API row onto the form shape for the Edit/View pages.
export const classFormFromRow = (row) => ({
  classId: row.classId,
  className: row.className || "",
  category1: row.category1Id || "",
  category2: row.category2Id || "",
  category3: row.category3Id || "",
  location: "",
  room: "",
  session: "",
  primaryInstructor: "",
  additionalInstructors: row.additionalInstructors || "",
  gender: row.gender || "",
  minAge: row.minAge ?? null,
  maxAge: row.maxAge ?? null,
  description: row.description || "",
  startDate: row.startDate ? row.startDate.substring(0, 10) : "",
  endDate: row.endDate ? row.endDate.substring(0, 10) : "",
  registrationOpenDate: row.registrationOpenDate ? row.registrationOpenDate.substring(0, 10) : "",
  cutoffDate: row.cutoffDate ? row.cutoffDate.substring(0, 10) : "",
  activeDays: row.activeDays || "",
  startTime: row.startTime || "",
  endTime: row.endTime || "",
  duration: formatDuration(row.startTime, row.endTime) || row.duration || "",
  tuitionFee: row.tuitionFee ?? null,
  billingMethod: row.billingMethod || "",
  billingCycle: row.billingCycle || "",
  registrationFee: !!row.registrationFee,
  dropInFee: !!row.dropInFee,
  maxClassSize: row.maxClassSize ?? null,
  maxWaitlistSize: row.maxWaitlistSize ?? null,
  allowWaitlistInRoll: !!row.allowWaitlistInRoll,
  allowWaitlistEnrollment: !!row.allowWaitlistEnrollment,
  allowDropIns: !!row.allowDropIns,
  makeupsInClass: !!row.makeupsInClass,
  virtualClassUrl: row.virtualClassUrl || "",
  linkDisplayText: row.linkDisplayText || "",
  onlineListings: !!row.onlineListings,
  onlineRegistration: !!row.onlineRegistration,
  allowPortalEnrollment: !!row.allowPortalEnrollment,
  parentPortalSchedule: !!row.parentPortalSchedule,
  allowPortalDropRequests: !!row.allowPortalDropRequests,
  policyGroups: row.policyGroups || "",
  active: row.active
});
