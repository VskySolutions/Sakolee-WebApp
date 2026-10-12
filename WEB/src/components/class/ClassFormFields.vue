<template>
  <div>
    <div class="form-section-title">Class Information</div>
    <div class="row q-col-gutter-md q-mb-md">
      <app-text-field v-model="form.className" label="Class Name *" placeholder="e.g. Advanced Ballet" class="col-12" :disable="disable" :rules="[requiredRule, classNameRule]" />
      <!-- Category 1/2/3, Location, Room, Session and Primary/Additional Instructors save to the Class record (backed by
           ClassCategory/Locations/ClassRooms/ClassSessions/the tenant's Staff users). -->
      <app-select :key="`category1-${categoriesLoaded}`" v-model="form.category1" label="Category 1 *" :options="category1Options" class="col-12 col-sm-6" :disable="disable" :rules="[requiredRule]" />
      <app-select :key="`category2-${categoriesLoaded}`" v-model="form.category2" label="Category 2" :options="category2Options" class="col-12 col-sm-6" :disable="disable" />
      <app-select :key="`category3-${categoriesLoaded}`" v-model="form.category3" label="Category 3" :options="category3Options" class="col-12 col-sm-6" :disable="disable" />
      <!-- Location must carry its own rule: Room is disabled until a location is picked, and a disabled
           field skips validation, so without this a class saves with neither. -->
      <app-select :key="`location-${categoriesLoaded}`" v-model="form.location" label="Location *" :options="locationOptions" class="col-12 col-sm-6" :disable="disable" :rules="[requiredRule]" />
      <app-select
        :key="`room-${roomsLoaded}`" v-model="form.room" label="Room *" :options="roomOptions" class="col-12 col-sm-6"
        :disable="disable || !form.location" :hint="form.location ? '' : 'Select a location first'" :rules="[requiredRule]"
      />
      <app-select :key="`session-${categoriesLoaded}`" v-model="form.session" label="Session *" :options="sessionOptions" class="col-12 col-sm-6" :disable="disable" />
      <app-select
        :key="`instructor-${categoriesLoaded}`"
        v-model="form.primaryInstructor"
        label="Primary Instructor *"
        :options="instructorOptions"
        class="col-12 col-sm-6"
        :disable="disable"
        :rules="[requiredRule]"
      />
      <app-select
        :key="`additional-instructors-${categoriesLoaded}`" v-model="form.additionalInstructors" label="Additional Instructors (Max 2)"
        :options="additionalInstructorOptions" multiple class="col-12 col-sm-6" :disable="disable"
      />
    </div>

    <q-separator class="q-my-sm" />
    <div class="form-section-title">Timeframe</div>
    <div class="row q-col-gutter-md q-mb-md">
      <!-- Row 1: Registration Open Date alone -->
      <app-date-field v-model="form.registrationOpenDate" label="Registration Open Date *" class="col-12 col-sm-6" :disable="disable" :rules="[requiredRule]" />
      <div class="row-break" />
      <!-- Row 2: Start Date + End Date side by side -->
      <app-date-field v-model="form.startDate" label="Start Date *" class="col-12 col-sm-6" :disable="disable || !form.registrationOpenDate" :hint="form.registrationOpenDate ? '' : 'Select registration open date first'" :options="startDateOptions" :default-year-month="startYearMonth" :rules="[requiredRule, startDateRule]" />
      <app-date-field v-model="form.endDate" label="End Date *" class="col-12 col-sm-6" :disable="disable || !form.startDate" :hint="form.startDate ? '' : 'Select start date first'" :options="endDateOptions" :default-year-month="endYearMonth" :rules="[requiredRule, endDateRule]" />
      <div class="col-12">
        <app-field-label label="Active Days *" />

        <div class="row q-gutter-sm">
          <q-btn
            v-for="day in dayOptions"
            :key="day.value"
            :label="day.label"
            rounded
            dense
            no-caps
            unelevated
            :color="isDaySelected(day.value) ? 'primary' : 'grey-3'"
            :text-color="isDaySelected(day.value) ? 'white' : 'grey-8'"
            :disable="disable"
            class="day-toggle"
            @click="toggleDay(day.value)"
          />
        </div>

        <div
          v-if="!disable && activeDaysTouched && selectedDays.length === 0"
          class="text-negative text-caption q-mt-xs"
        >
          Select at least one active day
        </div>
      </div>

      <!-- <app-time-field v-model="form.startTime" label="Start Time *" class="col-12 col-sm-6" :disable="disable" :rules="[requiredRule]" />
      <app-time-field v-model="form.endTime" label="End Time *" class="col-12 col-sm-6" :disable="disable" :rules="[requiredRule]" /> -->
      <app-time-field
        v-model="form.startTime"
        label="Start Time *"
        class="col-12 col-sm-6"
        :disable="disable"
        :rules="[requiredRule, startTimeRule]"
      />

      <app-time-field
        v-model="form.endTime"
        label="End Time *"
        class="col-12 col-sm-6"
        :disable="disable"
        :rules="[requiredRule, endTimeRule]"
      />
      <app-text-field v-model="form.duration" label="Duration" placeholder="—" hint="Calculated from Start Time and End Time" class="col-12 col-sm-6" disable />
    </div>

    <q-separator class="q-my-sm" />
    <div class="form-section-title">Pricing</div>
    <div class="row q-col-gutter-md q-mb-md">
      <app-text-field
        v-model.number="form.tuitionFee" label="Tuition Fee *" type="number" placeholder="0.00" class="col-12 col-sm-6"
        :disable="disable" :rules="[requiredRule]"
      >
        <template #prepend><span class="text-grey-7">$</span></template>
      </app-text-field>
      <app-select v-model="form.billingMethod" label="Billing Method" :options="billingMethodOptions" class="col-12 col-sm-6" :disable="disable" />
      <!-- <app-select v-model="form.billingCycle" label="Billing Cycle" :options="billingCycleOptions" class="col-12 col-sm-6" :disable="disable" /> -->
      <app-select v-model="form.billingCycle" label="Billing Cycle" :options="billingCycleOptions" class="col-12 col-sm-6" :disable="disable" />
      <div class="col-12 col-sm-6 toggle-row-inline">
        <q-toggle v-model="form.registrationFee" color="primary" :disable="disable" />
        <span class="q-ml-sm">Registration Fee Required</span>
      </div>
    </div>

    <q-separator class="q-my-sm" />
    <div class="form-section-title">Additional Information</div>
    <div class="row q-col-gutter-md q-mb-md">
      <app-text-field v-model="form.description" label="Description" type="textarea" placeholder="Enter class description..." class="col-12" :disable="disable" />
      <div class="col-12">
        <app-field-label label="Gender" />
        <q-option-group
          v-model="form.gender" :options="genderOptions" type="radio" inline dense color="primary"
          class="gender-options" :disable="disable"
        />
      </div>
      <app-text-field v-model.number="form.minAge" label="Min Age" type="number" placeholder="Min Age" class="col-12 col-sm-6" :disable="disable" required :rules="[ageRule, minAgeRule,requiredRules]" />
      <app-text-field v-model.number="form.maxAge" label="Max Age" type="number" placeholder="Max Age" class="col-12 col-sm-6" :disable="disable" required :rules="[requiredRules,ageRule, maxAgeRule]" />
      <!-- <app-text-field v-model.number="form.maxClassSize" label="Max Class Size" type="number" placeholder="Max Class Size" class="col-12 col-sm-6" :disable="disable" /> -->
      <app-text-field v-model.number="form.maxClassSize" label="Max Class Size *" type="number" placeholder="Enter maximum class size" class="col-12 col-sm-6" :disable="disable" :rules="[requiredRule]" />
      <app-text-field v-model.number="form.maxWaitlistSize" label="Max Waitlist Size" type="number" placeholder="Max Waitlist Size" class="col-12 col-sm-6" :disable="disable" />
      <app-date-field v-model="form.cutoffDate" label="Cutoff Date" class="col-12 col-sm-6" :disable="disable" />
      <app-select
        :key="`policies-${categoriesLoaded}`" v-model="form.policyIds" label="Policies" :options="policyOptions"
        multiple class="col-12 col-sm-6" :disable="disable" info="Managed under Settings → Policies."
      />
    </div>

    <q-separator class="q-my-sm" />
    <div class="form-section-title">Media</div>
    <div class="row q-col-gutter-md q-mb-md">
      <app-text-field v-model="form.virtualClassUrl" label="Virtual Class URL" placeholder="https://zoom.us/j/..." class="col-12 col-sm-6" :disable="disable" />
      <app-text-field v-model="form.linkDisplayText" label="Link Display Text" placeholder="Join Virtual Class" class="col-12 col-sm-6" :disable="disable" />
    </div>

    <q-separator class="q-my-sm" />
    <div class="form-section-title">Additional Settings</div>
    <div class="row q-col-gutter-md q-mb-md">
      <div class="col-12 col-sm-6 toggle-row"><span>Online Listings</span><q-toggle v-model="form.onlineListings" color="primary" :disable="disable" /></div>
      <div class="col-12 col-sm-6 toggle-row"><span>Parent Portal Schedule</span><q-toggle v-model="form.parentPortalSchedule" color="primary" :disable="disable" /></div>
      <div class="col-12 col-sm-6 toggle-row"><span>Online Registration</span><q-toggle v-model="form.onlineRegistration" color="primary" :disable="disable" /></div>
      <div class="col-12 col-sm-6 toggle-row"><span>Makeups in Class</span><q-toggle v-model="form.makeupsInClass" color="primary" :disable="disable" /></div>
      <div class="col-12 col-sm-6 toggle-row"><span>Allow Waitlist In Roll (Legacy Form)</span><q-toggle v-model="form.allowWaitlistInRoll" color="primary" :disable="disable" /></div>
      <div class="col-12 col-sm-6 toggle-row"><span>Allow Waitlist Enrollment (Parent Portal)</span><q-toggle v-model="form.allowWaitlistEnrollment" color="primary" :disable="disable" /></div>
      <div class="col-12 col-sm-6 toggle-row"><span>Allow Portal Enrollment</span><q-toggle v-model="form.allowPortalEnrollment" color="primary" :disable="disable" /></div>
      <div class="col-12 col-sm-6 toggle-row"><span>Allow Portal Drop Requests</span><q-toggle v-model="form.allowPortalDropRequests" color="primary" :disable="disable" /></div>
      <div class="col-12 col-sm-6 toggle-row"><span>Allow Drop-Ins</span><q-toggle v-model="form.allowDropIns" color="primary" :disable="disable" /></div>
      <div class="col-12 col-sm-6 toggle-row"><span>Drop-In Fee</span><q-toggle v-model="form.dropInFee" color="primary" :disable="disable" /></div>
    </div>

    <template v-if="showActiveToggle">
      <q-separator class="q-my-sm" />
      <div class="toggle-row-inline">
        <q-toggle v-model="form.active" color="primary" :disable="disable" />
        <span class="q-ml-sm">Active</span>
      </div>
    </template>
  </div>
</template>

<script setup>
// The Class create/edit/view field set, defined once and reused by the Add/Edit/View class pages.
import { ref, computed, watch, onMounted } from "vue";
import { classApi, classCategoryApi, classRoomApi, classSessionApi, locationApi, billingCycleApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { formatDuration } from "composables/classForm";

import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppDateField from "components/common/AppDateField.vue";
import AppTimeField from "components/common/AppTimeField.vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";

// The form object is shared via v-model; nested fields write back to the caller's reactive object.
const form = defineModel({ type: Object, required: true });

const props = defineProps({
  disable: { type: Boolean, default: false },
  // Only the Edit/View pages show this — a new class is always active on create.
  showActiveToggle: { type: Boolean, default: false },
});

const notify = useNotify();

// Category 1/2/3: real options loaded from ClassCategory (scoped to the caller's active tenant), one
// flat list told apart by categoryType — selections save to Class.Category1Id/2Id/3Id (see
// classForm.js's toClassPayload). Location/Session: the tenant's active Locations and ClassSessions,
// saved to Class.LocationId/SessionId. Primary Instructor: the tenant's active Staff users, saved to
// Class.PrimaryInstructorId. Room: the selected location's active ClassRooms, saved to Class.RoomId.
const classCategories = ref([]);
const locations = ref([]);
const sessions = ref([]);
const instructors = ref([]);
const billingCycles = ref([]);
// Policies: the tenant's active policies, saved to PolicyClassMapping (see classForm.js's toClassPayload).
const policies = ref([]);
// The Category/Location/Session selects mount before these async fetches resolve, and QSelect's value→label
// mapping (map-options) doesn't reliably re-run once `options` fills in later — an edit page showing
// a class's saved category renders the raw id instead of its name until this remounts them. Keying
// on this flag forces that remount the moment real options exist.
const categoriesLoaded = ref(false);
onMounted(async () => {
  // Loaded independently: one list failing must not leave the other empty.
  const [categoryResult, locationResult, sessionResult, instructorResult, policyResult] = await Promise.allSettled([
    classCategoryApi.list(),
    locationApi.list({ active: true }),
    classSessionApi.list({ isActive: true, limit: 100 }),
    classApi.instructors(),
    classApi.policies()
  ]);
  if (categoryResult.status === "fulfilled") classCategories.value = categoryResult.value?.data || [];
  else notify.error(getApiErrorMessage(categoryResult.reason));
  if (locationResult.status === "fulfilled") locations.value = locationResult.value?.data || [];
  else notify.error(getApiErrorMessage(locationResult.reason));
  if (sessionResult.status === "fulfilled") sessions.value = sessionResult.value?.data || [];
  else notify.error(getApiErrorMessage(sessionResult.reason));
  if (instructorResult.status === "fulfilled") instructors.value = instructorResult.value?.data || [];
  else notify.error(getApiErrorMessage(instructorResult.reason));
  if (policyResult.status === "fulfilled") policies.value = policyResult.value?.data || [];
  else notify.error(getApiErrorMessage(policyResult.reason));
  categoriesLoaded.value = true;
});

// Rooms belong to a location, so the Room list follows the selected Location. Bumping roomsLoaded remounts
// the Room select for the same reason categoriesLoaded does (saved id → name mapping).
const rooms = ref([]);
const roomsLoaded = ref(0);
const loadRooms = async (locationId) => {
  if (!locationId) {
    rooms.value = [];
  } else {
    try {
      const res = await classRoomApi.list({ locationId, limit: 100 });
      if (form.value.location !== locationId) return; // location changed again while loading
      rooms.value = (res?.data || []).filter((r) => r.active);
    } catch (err) {
      rooms.value = [];
      notify.error(getApiErrorMessage(err));
    }
  }
  roomsLoaded.value++;
};
watch(
  () => form.value.location,
  (locationId, previous) => {
    // Switching location drops a room picked under the old one (the edit page's initial fill comes from "").
    if (previous && locationId !== previous) form.value.room = "";
    loadRooms(locationId);
  },
  { immediate: true }
);

// The class's saved category is always offered under its name (from the class row itself), even when
// the loaded list lacks it — the list failed to load, or the class belongs to another tenant than the
// caller's active one. Without it the select falls back to showing the raw id.
const categoryOptions = (categoryType, field) => computed(() => {
  const options = classCategories.value
    .filter((c) => c.categoryType === categoryType)
    .map((c) => ({ label: c.name, value: c.classCategoryId }));
  const savedId = form.value[field];
  const savedName = form.value[`${field}Name`];
  if (savedId && savedName && !options.some((o) => o.value === savedId)) {
    options.unshift({ label: savedName, value: savedId });
  }
  return options;
});
const category1Options = categoryOptions("Category 1", "category1");
const category2Options = categoryOptions("Category 2", "category2");
const category3Options = categoryOptions("Category 3", "category3");
// Same fallback as the categories: a class whose saved location/session is now inactive (or not in the
// loaded list) still shows it by name.
const lookupOptions = (list, field) => computed(() => {
  const options = list.value.map((item) => ({ label: item.name, value: item.id }));
  const savedId = form.value[field];
  const savedName = form.value[`${field}Name`];
  if (savedId && savedName && !options.some((o) => o.value === savedId)) {
    options.unshift({ label: savedName, value: savedId });
  }
  return options;
});
const locationOptions = lookupOptions(locations, "location");
const sessionOptions = lookupOptions(sessions, "session");
const roomOptions = lookupOptions(rooms, "room");
const instructorOptions = lookupOptions(instructors, "primaryInstructor");
// Additional Instructors: the same Staff list, minus whoever is the primary instructor. Once 2 are
// picked the rest are disabled so the Max 2 limit can't be exceeded. Saved picks missing from the list
// still show by name (from the class row's additionalInstructorNames).
const additionalInstructorOptions = computed(() => {
  const selected = form.value.additionalInstructors || [];
  const options = instructors.value
    .filter((i) => i.id !== form.value.primaryInstructor)
    .map((i) => ({ label: i.name, value: i.id }));
  (form.value.additionalInstructorNames || []).forEach((saved) => {
    if (selected.includes(saved.id) && !options.some((o) => o.value === saved.id)) {
      options.unshift({ label: saved.name, value: saved.id });
    }
  });
  return options.map((o) => ({ ...o, disable: selected.length >= 2 && !selected.includes(o.value) }));
});
// Policies: the active list, plus any saved policy since made inactive (shown by name from the class row).
const policyOptions = computed(() => {
  const selected = form.value.policyIds || [];
  const options = policies.value.map((p) => ({ label: p.name, value: p.id }));
  (form.value.policyNames || []).forEach((saved) => {
    if (selected.includes(saved.id) && !options.some((o) => o.value === saved.id)) {
      options.push({ label: saved.name, value: saved.id });
    }
  });
  return options;
});
// Picking someone as primary drops them from the additional list, so nobody is listed twice.
watch(
  () => form.value.primaryInstructor,
  (primary) => {
    const additional = form.value.additionalInstructors || [];
    if (primary && additional.includes(primary)) {
      form.value.additionalInstructors = additional.filter((id) => id !== primary);
    }
  }
);

const genderOptions = [
  { label: "All", value: "All" },
  { label: "Male", value: "Male" },
  { label: "Female", value: "Female" }
];
const billingMethodOptions = ["Flat Rate", "Per Session", "Per Class", "Hourly"];
// const billingCycleOptions = ["Monthly", "Weekly", "Bi-Weekly", "Per Session", "One-Time"];
const billingCycleOptions = computed(() => billingCycles.value.map((cycle) => ({ label: cycle.name, value: cycle.name })));
// Active Days stays a comma-separated free-text string on the server (see Class entity); the pill
// toggles below just read/write that string so nothing on the wire changes.
const dayOptions = [
  { label: "Mon", value: "Mon" },
  { label: "Tue", value: "Tue" },
  { label: "Wed", value: "Wed" },
  { label: "Thu", value: "Thu" },
  { label: "Fri", value: "Fri" },
  { label: "Sat", value: "Sat" },
  { label: "Sun", value: "Sun" }
];
// Duration has no input of its own — it's always recomputed from the two time fields, so it can
// never disagree with them.
watch(
  () => [form.value.startTime, form.value.endTime],
  () => { form.value.duration = formatDuration(form.value.startTime, form.value.endTime); },
  { immediate: true }
);

// const selectedDays = computed(() => (form.value.activeDays || "").split(",").map((d) => d.trim()).filter(Boolean));
const selectedDays = computed(() =>
  (form.value.activeDays || "")
    .split(",")
    .map((d) => d.trim())
    .filter(Boolean)
);
const activeDaysTouched = ref(false);
const isDaySelected = (day) => selectedDays.value.includes(day);
// const toggleDay = (day) => {
//   if (props.disable) return;
//   const days = isDaySelected(day) ? selectedDays.value.filter((d) => d !== day) : [...selectedDays.value, day];
//   form.value.activeDays = dayOptions.map((d) => d.value).filter((d) => days.includes(d)).join(", ");
// };
const toggleDay = (day) => {
  if (props.disable) return;
  activeDaysTouched.value = true;
  const days = isDaySelected(day)
    ? selectedDays.value.filter((d) => d !== day)
    : [...selectedDays.value, day];
  form.value.activeDays = dayOptions
    .map((d) => d.value)
    .filter((d) => days.includes(d))
    .join(", ");
};

// Validations
const requiredRule = (v) => {
  if (Array.isArray(v)) {
    return v.length > 0 || "This field is required";
  }
  return (v !== null && v !== undefined && String(v).trim() !== "") || "This field is required";
};

const requiredRules = (val) => (val !== null && val !== undefined && String(val).trim() !== "") || "This field is required";
const timeToMinutes = (value) => {
  if (!value) return null;
  const text = String(value).trim();
  // 24-hour format: HH:mm
  let match = text.match(/^(\d{1,2}):(\d{2})$/);
  if (match) {
    const hours = Number(match[1]);
    const minutes = Number(match[2]);
    if (hours >= 0 && hours <= 23 && minutes >= 0 && minutes <= 59) {
      return hours * 60 + minutes;
    }
  }
  // 12-hour format: h:mm AM/PM
  match = text.match(/^(\d{1,2}):(\d{2})\s*(AM|PM)$/i);
  if (match) {
    let hours = Number(match[1]);
    const minutes = Number(match[2]);
    const period = match[3].toUpperCase();
    if (hours >= 1 && hours <= 12 && minutes >= 0 && minutes <= 59) {
      if (period === "AM") {
        if (hours === 12) hours = 0;
      } else if (hours !== 12) {
        hours += 12;
      }
      return hours * 60 + minutes;
    }
  }
  return null;
};

const startTimeRule = (value) => {
  if (!value) return true;
  const start = timeToMinutes(value);
  if (start === null) {
    return "Enter a valid start time";
  }
  if (form.value.endTime) {
    const end = timeToMinutes(form.value.endTime);
    if (end !== null && start >= end) {
      return "Start time must be before end time";
    }
  }
  return true;
};
const endTimeRule = (value) => {
  if (!value) return true;
  const end = timeToMinutes(value);
  if (end === null) {
    return "Enter a valid end time";
  }
  if (form.value.startTime) {
    const start = timeToMinutes(form.value.startTime);
    if (start !== null && end <= start) {
      return "End time must be after start time";
    }
  }
  return true;
};
// Min Age / Max Age: optional, whole numbers 0–120, and min must not exceed max
const hasValue = (v) => v !== "" && v !== null && v !== undefined;
const ageRule = (v) => !hasValue(v) || (Number.isInteger(Number(v)) && Number(v) >= 0 && Number(v) <= 120) || "Enter a whole number between 0 and 120";
const minAgeRule = (v) => !hasValue(v) || !hasValue(form.value.maxAge) || Number(v) <= Number(form.value.maxAge) || "Min age cannot be greater than max age";
const maxAgeRule = (v) => !hasValue(v) || !hasValue(form.value.minAge) || Number(v) >= Number(form.value.minAge) || "Max age cannot be less than min age";
// Class Name: letters (any language), spaces, and . ' - only. No digits.
const nameRegex = /^[\p{L}][\p{L}\s.'-]*$/u;
const classNameRule = (v) => !v || nameRegex.test(v.trim()) || "Only letters are allowed (no numbers)";
// Strip any disallowed character as the user types
watch(() => form.value.className, (v) => {
  if (typeof v !== "string") return;
  const cleaned = v.replace(/[^\p{L}\s.'-]/gu, "");
  if (cleaned !== v) form.value.className = cleaned;
});

// q-date `options` always receives "YYYY/MM/DD"; the model is "YYYY-MM-DD".
const toKey = (v) => (v ? String(v).slice(0, 10).replaceAll("-", "/") : "");

// Start Date: days BEFORE the registration open date are disabled
const startDateOptions = (d) =>
  !form.value.registrationOpenDate || d >= toKey(form.value.registrationOpenDate);

// End Date: days BEFORE the start date are disabled
const endDateOptions = (d) =>
  !form.value.startDate || d >= toKey(form.value.startDate);

const startDateRule = (v) =>
  !v || !form.value.registrationOpenDate ||
  toKey(v) >= toKey(form.value.registrationOpenDate) ||
  "Start date cannot be before the registration open date";

const endDateRule = (v) =>
  !v || !form.value.startDate ||
  toKey(v) >= toKey(form.value.startDate) ||
  "End date cannot be before the start date";

// Open each calendar on the month where selectable days begin ("YYYY/MM")
const startYearMonth = computed(() => toKey(form.value.registrationOpenDate).slice(0, 7) || undefined);
const endYearMonth = computed(() => toKey(form.value.startDate).slice(0, 7) || undefined);

// If an earlier date changes, clear later dates that are no longer valid
watch(() => form.value.registrationOpenDate, (reg) => {
  if (reg && form.value.startDate && toKey(form.value.startDate) < toKey(reg)) {
    form.value.startDate = "";
    form.value.endDate = "";
  }
});
watch(() => form.value.startDate, (start) => {
  if (start && form.value.endDate && toKey(form.value.endDate) < toKey(start)) {
    form.value.endDate = "";
  }
});
</script>

<style scoped>
.row-break {
  flex-basis: 100%;
  width: 100%;
  height: 0;
  padding: 0 !important;
  margin: 0 !important;
}
.form-section-title {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--q-primary);
  margin-bottom: 8px;
}
.toggle-row,
.toggle-row-inline {
  display: flex;
  align-items: center;
  min-height: 40px;
}
.toggle-row {
  justify-content: space-between;
}
.day-toggle {
  min-width: 56px;
}
.gender-options :deep(.q-radio) {
  margin-right: 16px;
}
</style>
