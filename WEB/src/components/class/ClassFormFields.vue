<template>
  <div>
    <div class="form-section-title">Class Information</div>
    <div class="row q-col-gutter-md q-mb-md">
      <app-text-field
        v-model="form.className" label="Class Name *" placeholder="e.g. Advanced Ballet" class="col-12 col-sm-6"
        :disable="disable" :rules="[(v) => !!v || 'Class name is required']"
      />

      <!-- Category 1/2/3 save to the Class record (backed by ClassCategory). Location/Room/Session/
           Instructor are still placeholder option lists — there is no management feature for them
           yet, so those don't save to the class record until that backing data exists. -->
      <app-select v-model="form.category1" label="Category 1 *" :options="category1Options" class="col-12 col-sm-6" :disable="disable" />
      <app-select v-model="form.category2" label="Category 2" :options="category2Options" class="col-12 col-sm-6" :disable="disable" />
      <app-select v-model="form.category3" label="Category 3" :options="category3Options" class="col-12 col-sm-6" :disable="disable" />
      <app-select v-model="form.location" label="Location *" :options="locationOptions" class="col-12 col-sm-6" :disable="disable" />
      <app-select v-model="form.room" label="Room *" :options="roomOptions" class="col-12 col-sm-6" :disable="disable" />
      <app-select v-model="form.session" label="Session *" :options="sessionOptions" class="col-12 col-sm-6" :disable="disable" />
      <app-select v-model="form.primaryInstructor" label="Primary Instructor *" :options="instructorOptions" class="col-12 col-sm-6" :disable="disable" />
      <app-text-field v-model="form.additionalInstructors" label="Additional Instructors (Max 2)" hint="Free text (no instructor list yet)" class="col-12 col-sm-6" :disable="disable" />
    </div>

    <q-separator class="q-my-sm" />
    <div class="form-section-title">Timeframe</div>
    <div class="row q-col-gutter-md q-mb-md">
      <app-date-field v-model="form.startDate" label="Start Date *" class="col-12 col-sm-6" :disable="disable" />
      <app-date-field v-model="form.endDate" label="End Date *" class="col-12 col-sm-6" :disable="disable" />
      <app-date-field v-model="form.registrationOpenDate" label="Registration Open Date *" class="col-12 col-sm-6" :disable="disable" />

      <div class="col-12">
        <app-field-label label="Active Days" />
        <div class="row q-gutter-sm">
          <q-btn
            v-for="day in dayOptions" :key="day.value"
            :label="day.label" rounded dense no-caps unelevated
            :color="isDaySelected(day.value) ? 'primary' : 'grey-3'"
            :text-color="isDaySelected(day.value) ? 'white' : 'grey-8'"
            :disable="disable"
            class="day-toggle"
            @click="toggleDay(day.value)"
          />
        </div>
      </div>

      <app-time-field v-model="form.startTime" label="Start Time *" class="col-12 col-sm-6" :disable="disable" />
      <app-time-field v-model="form.endTime" label="End Time *" class="col-12 col-sm-6" :disable="disable" />
      <app-text-field v-model="form.duration" label="Duration" placeholder="—" hint="Calculated from Start Time and End Time" class="col-12 col-sm-6" disable />
    </div>

    <q-separator class="q-my-sm" />
    <div class="form-section-title">Pricing</div>
    <div class="row q-col-gutter-md q-mb-md">
      <app-text-field
        v-model.number="form.tuitionFee" label="Tuition Fee *" type="number" placeholder="0.00" class="col-12 col-sm-6"
        :disable="disable" :rules="[(v) => (v !== null && v !== '') || 'Tuition fee is required']"
      >
        <template #prepend><span class="text-grey-7">$</span></template>
      </app-text-field>
      <app-select v-model="form.billingMethod" label="Billing Method" :options="billingMethodOptions" class="col-12 col-sm-6" :disable="disable" />
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
      <app-text-field v-model.number="form.minAge" label="Min Age" type="number" placeholder="Min Age" class="col-12 col-sm-6" :disable="disable" />
      <app-text-field v-model.number="form.maxAge" label="Max Age" type="number" placeholder="Max Age" class="col-12 col-sm-6" :disable="disable" />
      <app-text-field v-model.number="form.maxClassSize" label="Max Class Size" type="number" placeholder="Max Class Size" class="col-12 col-sm-6" :disable="disable" />
      <app-text-field v-model.number="form.maxWaitlistSize" label="Max Waitlist Size" type="number" placeholder="Max Waitlist Size" class="col-12 col-sm-6" :disable="disable" />
      <app-date-field v-model="form.cutoffDate" label="Cutoff Date" class="col-12 col-sm-6" :disable="disable" />
      <app-text-field v-model="form.policyGroups" label="Policy Groups" hint="Free text (no policy group list yet)" class="col-12 col-sm-6" :disable="disable" />
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
import { classCategoryApi, getApiErrorMessage } from "services/api";
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
  showActiveToggle: { type: Boolean, default: false }
});

const notify = useNotify();

// Category 1/2/3: real options loaded from ClassCategory (scoped to the caller's active tenant), one
// flat list told apart by categoryType — selections save to Class.Category1Id/2Id/3Id (see
// classForm.js's toClassPayload). Location/Room/Session/Instructor stay demo option lists standing in
// for the not-yet-built management features and are not sent on submit.
const classCategories = ref([]);
onMounted(async () => {
  try {
    classCategories.value = await classCategoryApi.list() || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
});

const categoryOptions = (categoryType) => computed(() =>
  classCategories.value
    .filter((c) => c.categoryType === categoryType)
    .map((c) => ({ label: c.name, value: c.classCategoryId })));
const category1Options = categoryOptions("Category 1");
const category2Options = categoryOptions("Category 2");
const category3Options = categoryOptions("Category 3");
const locationOptions = ["North Studio", "Downtown Campus", "Westside Academy"];
const roomOptions = ["Studio A", "Studio B", "Main Gym"];
const sessionOptions = ["Spring 2024", "Fall 2024", "Winter 2025"];
const instructorOptions = ["Sarah Jenkins", "Michael Chen", "Elena Rodriguez"];

const genderOptions = [
  { label: "All", value: "All" },
  { label: "Male", value: "Male" },
  { label: "Female", value: "Female" }
];
const billingMethodOptions = ["Flat Rate", "Per Session", "Per Class", "Hourly"];
const billingCycleOptions = ["Monthly", "Weekly", "Bi-Weekly", "Per Session", "One-Time"];

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

const selectedDays = computed(() => (form.value.activeDays || "").split(",").map((d) => d.trim()).filter(Boolean));
const isDaySelected = (day) => selectedDays.value.includes(day);
const toggleDay = (day) => {
  if (props.disable) return;
  const days = isDaySelected(day) ? selectedDays.value.filter((d) => d !== day) : [...selectedDays.value, day];
  form.value.activeDays = dayOptions.map((d) => d.value).filter((d) => days.includes(d)).join(", ");
};
</script>

<style scoped>
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
