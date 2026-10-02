<template>
  <app-form-dialog
    v-model="isOpen"
    :title="classData?.className ? `View ${classData.className}` : 'View Class'"
    size="xl"
    hide-footer
    @cancel="resetView"
  >
    <div class="view-class-scss row q-col-gutter-lg">
      <q-inner-loading :showing="loading" />

      <!-- =========================================================
           LEFT COLUMN
           ========================================================= -->
      <div class="col-12 col-md-7">
        <div class="cv-stack">

          <!-- ================= CLASS INFORMATION ================= -->
          <div class="cv-card">
            <div class="cv-card__title">
              <q-icon name="o_info" size="18px" color="primary" />
              <span class="fw-700 fs-11 text-86">
                CLASS INFORMATION
              </span>
            </div>

            <div class="row q-col-gutter-md">

              <div class="col-6">
                <div class="cv-label">Class Name</div>
                <div class="cv-value">
                  {{ classData?.className || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Status</div>
                <span
                  :class="classData?.active ? 'cv-pill' : 'cv-pill cv-pill--grey'"
                >
                  {{ classData?.active ? "Active" : "Inactive" }}
                </span>
              </div>

              <div class="col-6">
                <div class="cv-label">Category 1</div>
                <div class="cv-value">
                  {{ classData?.category1Name || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Category 2</div>
                <div class="cv-value">
                  {{ classData?.category2Name || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Category 3</div>
                <div class="cv-value">
                  {{ classData?.category3Name || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Location</div>
                <span
                  v-if="classData?.locationName"
                  class="cv-pill"
                >
                  {{ classData.locationName }}
                </span>
                <div v-else class="cv-value">—</div>
              </div>

              <div class="col-6">
                <div class="cv-label">Room</div>
                <div class="cv-value">
                  {{ classData?.roomName || classData?.roomId || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Session</div>
                <span
                  v-if="classData?.sessionName"
                  class="cv-pill"
                >
                  {{ classData.sessionName }}
                </span>
                <div v-else class="cv-value">—</div>
              </div>

            </div>
          </div>

          <!-- =============== CLASS DETAILS & ENROLLMENTS =============== -->
          <div class="cv-card">
            <div class="cv-card__title">
              <!-- <q-icon name="o_school" size="18px" color="primary" /> -->
              <span class="material-symbols-outlined text-18 text-d4">badge</span>
              <span class="fw-700 fs-11 text-86">
                CLASS DETAILS & ENROLLMENTS
              </span>
            </div>

            <q-separator class="view-separator q-mb-md" />

            <div class="row q-col-gutter-md">

              <div class="col-6">
                <div class="cv-label">Start Date</div>
                <div class="cv-value">
                  {{ formatLongDate(classData?.startDate) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">End Date</div>
                <div class="cv-value">
                  {{ formatLongDate(classData?.endDate) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Registration Start Date</div>
                <div class="cv-value">
                  {{ formatLongDate(classData?.registrationOpenDate) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Cutoff Date</div>
                <div class="cv-value">
                  {{ formatLongDate(classData?.cutoffDate) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Gender</div>
                <div class="cv-value">
                  {{ classData?.gender || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Age Range</div>
                <div class="cv-value">
                  {{ formatAgeRange(classData?.minAge, classData?.maxAge) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Min Age</div>
                <div class="cv-value">
                  {{ classData?.minAge || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Max Age</div>
                <div class="cv-value">
                  {{ classData?.maxAge || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Max Class Size</div>
                <span class="cv-pill">
                  {{ classData?.maxClassSize || "—" }}
                </span>
              </div>

              <div class="col-6">
                <div class="cv-label">Max Waitlist</div>
                <span class="cv-pill">
                  {{ classData?.maxWaitlistSize || "—" }}
                </span>
              </div>

              <!-- <div class="col-6">
                <div class="cv-label">Active Days</div>
                <div class="cv-value">
                  {{ classData?.activeDays || "—" }}
                </div>
              </div> -->

              <div class="col-6">
                <div class="cv-label">Duration</div>
                <div class="cv-value">
                  {{ classData?.duration || "—" }}
                </div>
              </div>

              <!-- <div class="col-12">
                <div class="cv-label">Additional Instructors</div>
                <div class="cv-value">
                  {{ additionalInstructorNames }}
                </div>
              </div> -->

              <div class="col-12">
                <div class="cv-label">Description</div>
                <div class="cv-value cv-value--multiline">
                  {{ classData?.description || "—" }}
                </div>
              </div>

              <div class="col-12">
                <div class="cv-label">Policy Groups</div>
                <div class="cv-value cv-value--multiline">
                  {{ classData?.policyGroups || "—" }}
                </div>
              </div>

            </div>
          </div>

          <!-- =========================== URLs =========================== -->
          <div class="cv-card">
            <div class="cv-card__title">
              <q-icon name="o_link" size="18px" color="primary" />
              <span class="fw-700 fs-11 text-86">
                URLS
              </span>
            </div>

            <div class="cv-label">
              Virtual Class / Video Link URL
            </div>

            <a
              v-if="classData?.virtualClassUrl"
              :href="classData.virtualClassUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="cv-link"
            >
              {{ classData.virtualClassUrl }}
            </a>

            <div v-else class="cv-value">—</div>

            <div class="cv-label q-mt-md">
              Virtual Class / Video Link Text
            </div>

            <div class="cv-value">
              {{ classData?.linkDisplayText || "—" }}
            </div>
          </div>

          <!-- ====================== AUDIT INFORMATION ====================== -->
          <div class="cv-card">
            <div class="cv-card__title">
              <q-icon name="o_history" size="18px" color="primary" />
              <span class="fw-700 fs-11 text-86">
                AUDIT INFORMATION
              </span>
            </div>

            <div class="row q-col-gutter-md">

              <div class="col-6">
                <div class="cv-label">Created By</div>
                <div class="cv-value">
                  {{ classData?.createdBy || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Created On</div>
                <div class="cv-value">
                  {{ formatLongDateTime(classData?.createdOnUtc) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Updated By</div>
                <div class="cv-value">
                  {{ classData?.updatedBy || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Updated On</div>
                <div class="cv-value">
                  {{ formatLongDateTime(classData?.updatedOnUtc) }}
                </div>
              </div>

            </div>
          </div>

        </div>
      </div>

      <!-- =========================================================
           RIGHT COLUMN
           ========================================================= -->
      <div class="col-12 col-md-5">
        <div class="cv-stack">

          <!-- ========================== SCHEDULE ========================== -->
          <div class="cv-card">
            <div class="cv-card__title">
              <!-- <q-icon name="o_calendar_month" size="18px" color="primary" /> -->
              <span class="material-symbols-outlined text-18 text-d4">calendar_today</span>
              <span class="fw-700 fs-11 text-86">
                SCHEDULE
              </span>
            </div>

            <div class="cv-schedule">
              <div class="cv-schedule__days">
                {{ classData?.activeDays || "—" }}
              </div>

              <div class="cv-schedule__time">
                {{ classData?.startTime || "—" }}
                <span
                  v-if="classData?.startTime && classData?.endTime"
                >
                  -
                </span>
                {{ classData?.endTime || "" }}

                <span
                  v-if="classData?.duration"
                  class="q-ml-xs"
                >
                  ({{ classData.duration }})
                </span>
              </div>
            </div>
          </div>

          <!-- ==================== INSTRUCTOR INFORMATION ==================== -->
          <div class="cv-card">
            <div class="cv-card__title">
              <q-icon name="o_person" size="18px" color="primary" />
              <span class="fw-700 fs-11 text-86">
                INSTRUCTOR INFORMATION
              </span>
            </div>

            <div class="cv-label">Primary Instructor</div>

            <div class="cv-instructor">
              <span class="cv-avatar">
                {{ instructorInitials }}
              </span>

              <span class="cv-value">
                {{ classData?.primaryInstructorName || "—" }}
              </span>
            </div>

            <q-separator class="q-my-md view-separator" />

            <div class="cv-label">Additional Instructors</div>

            <div class="cv-value">
              {{ additionalInstructorNames }}
            </div>
          </div>

          <!-- =========================== PRICING =========================== -->
          <div class="cv-card">
            <div class="cv-card__title">
              <!-- <q-icon name="o_payments" size="18px" color="primary" /> -->
              <span class="material-symbols-outlined text-18 text-d4">payments</span>
              <span class="fw-700 fs-11 text-86">
                PRICING
              </span>
            </div>

            <div class="row q-col-gutter-md">

              <div class="col-6">
                <div class="cv-label">Tuition Fee</div>
                <div class="cv-value font-mono">
                  {{ formatCurrency(classData?.tuitionFee) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Billing Cycle</div>

                <span
                  v-if="classData?.billingCycle"
                  class="cv-pill"
                >
                  {{ classData.billingCycle }}
                </span>

                <div v-else class="cv-value">—</div>
              </div>

              <div class="col-6">
                <div class="cv-label">Billing Method</div>
                <div class="cv-value">
                  {{ classData?.billingMethod || "—" }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Registration Fee</div>
                <span class="cv-pill">
                  {{ yesNo(classData?.registrationFee) }}
                </span>
              </div>

              <div class="col-6">
                <div class="cv-label">Drop-In Fee</div>
                <span class="cv-pill">
                  {{ yesNo(classData?.dropInFee) }}
                </span>
              </div>

            </div>
          </div>

          <!-- ====================== PORTAL & REGISTRATION ====================== -->
          <div class="cv-card">

            <div class="cv-card__title">
              <!-- <q-icon name="o_tune" size="18px" color="primary" /> -->
              <span class="material-symbols-outlined text-18 text-d4">devices</span>
              <span class="fw-700 fs-11 text-86">
                PORTAL & REGISTRATION
              </span>
            </div>

            <div class="row q-col-gutter-x-lg">

              <!-- Left -->
              <div class="col-12 col-sm-6">

                <div class="cv-setting-row">
                  <span>Display in Listings</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.onlineListings }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

                <div class="cv-setting-row">
                  <span>Parent Portal Schedule</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.parentPortalSchedule }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

                <div class="cv-setting-row">
                  <span>Makeups In Class</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.makeupsInClass }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

                <div class="cv-setting-row">
                  <span>Allow Waitlist In Roll</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.allowWaitlistInRoll }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

                <div class="cv-setting-row">
                  <span>Allow Drop-Ins</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.allowDropIns }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

              </div>

              <!-- Right -->
              <div class="col-12 col-sm-6">

                <div class="cv-setting-row">
                  <span>Online Registration</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.onlineRegistration }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

                <div class="cv-setting-row">
                  <span>Portal Enrollment</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.allowPortalEnrollment }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

                <div class="cv-setting-row">
                  <span>Allow Waitlist Enrollment</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.allowWaitlistEnrollment }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

                <div class="cv-setting-row">
                  <span>Allow Portal Drop Requests</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.allowPortalDropRequests }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

                <div class="cv-setting-row">
                  <span>Drop-In Fee</span>
                  <span
                    class="cv-switch"
                    :class="{ 'cv-switch--on': classData?.dropInFee }"
                  >
                    <span class="cv-switch__dot" />
                  </span>
                </div>

              </div>

            </div>

          </div>

          <!-- ====================== ADDITIONAL SETTINGS ====================== -->
          <!-- <div class="cv-card">

            <div class="cv-card__title">
              <q-icon name="o_settings" size="18px" color="primary" />
              <span class="fw-700 fs-11 text-86">
                ADDITIONAL SETTINGS
              </span>
            </div>

            <div class="row q-col-gutter-md">

              <div class="col-6">
                <div class="cv-label">Online Listings</div>
                <div class="cv-value">
                  {{ yesNo(classData?.onlineListings) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Online Registration</div>
                <div class="cv-value">
                  {{ yesNo(classData?.onlineRegistration) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Portal Enrollment</div>
                <div class="cv-value">
                  {{ yesNo(classData?.allowPortalEnrollment) }}
                </div>
              </div>

              <div class="col-6">
                <div class="cv-label">Parent Portal Schedule</div>
                <div class="cv-value">
                  {{ yesNo(classData?.parentPortalSchedule) }}
                </div>
              </div>

            </div>

          </div> -->

        </div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import { classApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useDateFormat } from "composables/useDateFormat";

import AppFormDialog from "components/common/AppFormDialog.vue";

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },

  recordId: {
    type: [String, Number],
    default: null
  }
});

const emit = defineEmits(["update:modelValue"]);
const { formatLongDate, formatLongDateTime } = useDateFormat();
const notify = useNotify();

const isOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit("update:modelValue", value)
});

const classData = ref(null);
const loading = ref(false);

const resetView = () => {
  classData.value = null;
};

const load = async () => {
  if (!props.recordId) {
    return;
  }

  loading.value = true;

  try {
    classData.value = await classApi.get(props.recordId);
  } catch (err) {
    classData.value = null;
    isOpen.value = false;
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
};

watch(
  () => props.modelValue,
  async (value) => {
    if (!value) {
      resetView();
      return;
    }

    await load();
  }
);

const formatCurrency = (value) => {
  if (value === null || value === undefined || value === "") {
    return "—";
  }

  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD"
  }).format(value);
};

const formatAgeRange = (min, max) => {
  if (min == null && max == null) return "—";
  if (min == null) return `${max}+ yrs`;
  if (max == null) return `${min}+ yrs`;

  return `${min} – ${max} yrs`;
};

const yesNo = (value) => {
  return value ? "Yes" : "No";
};

const additionalInstructorNames = computed(() => {
  const instructors =
    classData.value?.additionalInstructors || [];

  if (!instructors.length) {
    return "—";
  }

  return instructors
    .map((item) => item?.name)
    .filter(Boolean)
    .join(", ") || "—";
});

const instructorInitials = computed(() => {
  const name =
    classData.value?.primaryInstructorName || "";

  const parts = name
    .trim()
    .split(/\s+/)
    .filter(Boolean);

  if (!parts.length) {
    return "?";
  }

  if (parts.length === 1) {
    return parts[0].charAt(0).toUpperCase();
  }

  return (
    parts[0].charAt(0) +
    parts[parts.length - 1].charAt(0)
  ).toUpperCase();
});
</script>
