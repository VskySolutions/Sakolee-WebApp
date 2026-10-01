<template>
  <!-- Read-only View Family dialog (same app-form-dialog pattern as the Studio Locations view) -->
  <app-form-dialog
    v-model="isOpen"
    :title="form.familyName ? `View ${form.familyName}` : 'View Family'"
    size="xl"
    hide-footer
    @cancel="resetView"
  >
    <div class="view-family-scss row q-col-gutter-lg">
      <q-inner-loading :showing="loading" />

      <!-- ================= Left column ================= -->
      <div class="col-12 col-md-7">
        <div class="fv-stack">
          <!-- Family Information -->
          <div class="fv-card">
            <div class="fv-card__title">
              <span class="material-symbols-outlined fs-18 text-d4">family_restroom</span>
              <span class="fw-700 fs-11 text-86">Family Information</span>
            </div>

            <div class="fw-500 fs-11 text-86  mb-4">Family Name</div>
            <div class="fw-700 fs-16 text-2e">{{ form.familyName || "—" }}</div>

            <q-separator class="q-my-md view-separator" />

            <div class="row q-col-gutter-md">
              <div class="col-6">
                <div class="fw-500 fs-11 text-86  mb-4">Location</div>
                <span v-if="form.studioLocationName" class="fv-pill fw-600 fs-12">{{ form.studioLocationName }}</span>
                <div v-else class="fv-value">—</div>
              </div>

              <div class="col-6">
                <div class="fw-500 fs-11 text-86  mb-4">Status</div>
                <span :class="form.active ? 'fv-pill' : 'fv-pill fv-pill--grey'">
                  {{ form.active ? "Active" : "Inactive" }}
                </span>
              </div>

              <div class="col-6">
                <div class="fw-500 fs-11 text-86  mb-4">Registration Date</div>
                <div class="fw-600 fs-12 text-2e">{{ formatLongDate(meta.createdOnUtc) }}</div>
              </div>

              <!-- <div class="col-6">
                <div class="fw-500 fs-11 text-86 mb-4">Family Status</div>
                <div class="fv-value">{{ familyStatusLabel }}</div>
              </div> -->
            </div>
          </div>

          <!-- Enrolled Students -->
          <div class="fv-card">
            <div class="fv-card__title">
              <q-icon name="o_school" size="18px" color="primary" />
              <span class="fw-700 fs-11 text-86">Enrolled Students</span>
              <q-space />
              <span class="fv-count">{{ activeStudentCount }} Active</span>
            </div>
            <q-separator class="view-separator" />
            <template v-if="students.length">
              <div class="fv-table">
                <div class="fv-table__row fv-table__head">
                  <div>Student Name</div>
                  <div>DOB</div>
                  <div>Status</div>
                </div>

                <div
                  v-for="student in students"
                  :key="student.studentId"
                  class="fv-table__row"
                >
                  <div class="row items-center no-wrap q-gutter-x-sm">
                    <span class="fv-avatar">{{ initials(student.firstName, student.lastName) }}</span>
                    <span class="fw-600 text-2e fs-12 ellipsis">{{ student.firstName }}</span>
                  </div>
                  <div class="text-86 fs-12">{{ formatLongDate(student.birthDate) || "—" }}</div>
                  <div>
                    <span class="fw-600 fs-12 text-d4" :class="student.active ? 'fv-dot' : 'fv-dot fv-dot--grey'">
                      {{ student.active ? "Active" : "Inactive" }}
                    </span>
                  </div>
                </div>
              </div>
            </template>

            <div v-else class="fv-empty">No students enrolled yet.</div>
          </div>

          <!-- Secondary Contact + Referral Details -->
          <div class="fv-pair">
            <div>
              <div class="fv-card full-height">
                <div class="fv-card__title">
                  <span class="material-symbols-outlined text-primary fs-18 text-d4">credit_card</span>
                  <span class="fs-11 fw-700 text-86">Billing Details</span>
                </div>

                <div class="fv-row">
                  <span class="fs-12 text-86">Method</span>
                  <span class="fs-12 fw-700 text-23 text-right">{{ form.method || "—" }}</span>
                </div>
                <div class="fv-row">
                  <span class="fs-12 text-86">Auto Pay</span>
                  <span class="fs-12 fw-700 text-23 text-right">{{ form.autoPay || "—" }}</span>
                </div>

                <q-separator class="q-my-sm view-separator" />

                <div class="fs-10 text-86">Billing Note</div>
                <div class="fs-11 text-54 q-mt-xs">{{ form.billingNote || "" }}</div>
              </div>
            </div>
            <div>
              <div class="fv-card full-height">
                <div class="fv-card__title">
                  <span class="material-symbols-outlined text-primary fs-18 text-d4">verified</span>
                  <span class="fs-11 fw-700 text-86">AGREEMENTS</span>
                </div>

                <!-- <template v-if="form.secondaryContact">
                  <div class="fv-value fw-700">
                    {{ fullName(form.secondaryContact.firstName, form.secondaryContact.lastName) }}
                  </div>
                  <div class="fv-label q-mb-sm">{{ form.secondaryContact.relation || "—" }}</div>

                  <div class="fv-line">
                    <q-icon name="o_mail" size="16px" color="primary" />
                    <span class="ellipsis" :title="form.secondaryContact.email">
                      {{ form.secondaryContact.email || "—" }}
                    </span>
                  </div>
                  <div class="fv-line">
                    <q-icon name="o_smartphone" size="16px" color="primary" />
                    {{ form.secondaryContact.phone || "—" }}
                  </div>

                  <div class="q-mt-sm column q-gutter-y-xs items-start">
                    <span v-if="form.secondaryContact.isBillingContact" class="fv-chip">
                      <q-icon name="o_check_circle" size="13px" /> Billing Contact
                    </span>
                    <span v-if="form.secondaryContact.isAuthorizedToPickUpStudent" class="fv-chip">
                      <q-icon name="o_check_circle" size="13px" /> Authorized Pick Up
                    </span>
                  </div>
                </template> -->

                <!--Remove v-else -->
                <div class="fv-empty">No Agreement yet.</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ================= Right column ================= -->
      <div class="col-12 col-md-5">
        <div class="fv-stack">
          <!-- Primary Contact -->
          <div class="fv-card">
            <div class="fv-card__title">
              <q-icon name="o_person" size="18px" color="primary" />
              <span class="fw-700 fs-11 text-86">Primary Contact</span>
            </div>

            <div class="row q-col-gutter-sm">
              <div class="col-6">
                <div class="fw-500 fs-11 text-86  mb-4">First Name</div>
                <div class="fw-600 fs-12 text-2e">{{ form.firstName || "—" }}</div>
              </div>
              <div class="col-6">
                <div class="fw-500 fs-11 text-86  mb-4">Last Name</div>
                <div class="fw-600 fs-12 text-2e">{{ form.lastName || "—" }}</div>
              </div>
              <div class="col-12">
                <div class="fw-500 fs-11 text-86  mb-4">Relationship</div>
                <div class="fw-600 fs-12 text-2e">{{ form.relation || "—" }}</div>
              </div>
              <div class="col-6">
                <div class="fw-500 fs-11 text-86  mb-4">Preferred Name</div>
                <div class="fw-600 fs-12 text-2e">{{ form.referralName || "—" }}</div>
              </div>
              <div class="col-6">
                <div class="fw-500 fs-11 text-86  mb-4">DOB</div>
                <div class="fw-600 fs-12 text-2e">{{ form.birthDate || "—" }}</div>
              </div>
            </div>

            <q-separator class="q-my-md" />

            <div class="fv-card__title q-mb-sm fw-700 fs-11 text-86">
              Contact Info
              <q-space />
              <span v-if="form.isBillingContact" class="fv-count">Prefers Email</span>
            </div>

            <div class="fv-line">
              <q-icon name="o_mail" size="16px" color="primary" />
              <span class="ellipsis fs-12 text-54" :title="form.email">{{ form.email || "—" }}</span>
            </div>
            <div v-for="phone in primaryPhones" :key="phone.label" class="fv-line">
              <q-icon :name="phone.icon" size="16px" color="primary" />
              <span class="fs-12 text-54">{{ phone.value }}</span>
              <span class="fs-12 text-54">({{ phone.label }})</span>
            </div>
            <div class="fv-line items-start">
              <q-icon name="o_place" size="16px" color="primary" class="q-mt-xs" />
              <div class="fs-13">
                <template v-if="addressLines.length">
                  <div v-for="line in addressLines" :key="line" class="fs-12 text-54">{{ line }}</div>
                </template>
                <template v-else>—</template>
              </div>
            </div>

            <!-- <div v-if="form.isAuthorizedToPickUpStudent" class="q-mt-sm">
              <span class="fv-chip">
                <q-icon name="o_check_circle" size="13px" /> Authorized Pick Up
              </span>
            </div> -->
          </div>

          <!-- Emergency Contact -->
          <div class="fv-card fv-card--danger">
            <div class="fv-card__title text-negative">
              <span class="material-symbols-outlined fs-18 text-48">emergency</span>
              <span class="fs-11 text-48">Emergency Contact</span>
            </div>

            <div class="fw-700 fs-12 text-2e">{{ form.emergencyContactPerson || "—" }}</div>

            <div class="fv-line q-mt-xs">
              <q-icon name="o_call" size="16px" color="primary" />
              <span class="fw-500 fs-12 text-2e">{{ form.emergencyPhone || "—" }}</span>
            </div>
          </div>

          <!-- Preferences -->
          <div class="fv-card">
            <div class="fv-card__title">
              <span class="material-symbols-outlined fs-18 text-d4">tune</span>
              <span class="fs-11 text-86">PREFERENCES</span>
            </div>
            <span class="fs-12 text-86 text-right">No Preferences yet.</span>
          </div>
        </div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { familyApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useDateFormat } from "composables/useDateFormat";
import { blankFamilyForm, familyFormFromDetail } from "composables/familyForm";

import AppFormDialog from "components/common/AppFormDialog.vue";

const props = defineProps({
  modelValue: { type: Boolean, required: true },
  familyId: { type: [String, Number], default: null },
  // Owned by the list page (it also drives the Family Status filter there).
  familyStatusOptions: { type: Array, default: () => [] }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const { tenantTimeZone } = useDateFormat();

// "Oct 24, 2023" in the tenant's time zone (UTC assumed when the value has no designator).
const formatLongDate = (value) => {
  if (!value) return "—";
  let s = String(value);
  if (!/[zZ]$|[+-]\d{2}:?\d{2}$/.test(s)) s += "Z";
  const d = new Date(s);
  if (Number.isNaN(d.getTime())) return "—";
  return new Intl.DateTimeFormat("en-US", {
    timeZone: tenantTimeZone(),
    month: "short",
    day: "numeric",
    year: "numeric"
  }).format(d);
};

const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

const form = reactive(blankFamilyForm());
const students = ref([]);
const loading = ref(false);
// Detail-only fields that aren't part of the shared form shape.
const meta = reactive({ createdOnUtc: null, familyStatusName: "" });

// const familyStatusLabel = computed(() =>
//   meta.familyStatusName ||
//   props.familyStatusOptions.find((x) => x.value === form.familyStatusId)?.label ||
//   "—"
// );

const activeStudentCount = computed(() => students.value.filter((s) => s.active).length);

const primaryPhones = computed(() =>
  [
    { label: "Cell", icon: "o_smartphone", value: form.cellPhone },
    { label: "Home", icon: "o_call", value: form.homePhone },
    { label: "Work", icon: "o_call", value: form.workPhone },
    { label: "Other", icon: "o_call", value: form.otherPhone },
    { label: "Fax", icon: "o_fax", value: form.fax }
  ].filter((p) => p.value)
);

const addressLines = computed(() => {
  const cityLine = [form.city, [form.state, form.zipCode].filter(Boolean).join(" ")]
    .filter(Boolean)
    .join(", ");
  return [form.address1, form.address2, cityLine].filter(Boolean);
});

const fullName = (first, last) => `${first || ""} ${last || ""}`.trim() || "—";

// const studentName = (s) =>
//   s.firstName || s.lastName ? fullName(s.firstName, s.lastName) : s.studentNumber || "Student";

const initials = (first, last) =>
  `${(first || "").charAt(0)}${(last || "").charAt(0)}`.toUpperCase() || "?";

const resetView = () => {
  Object.assign(form, blankFamilyForm());
  students.value = [];
  meta.createdOnUtc = null;
  meta.familyStatusName = "";
};

// Each time the dialog opens, load the family fresh; close again if it can't be loaded.
watch(
  () => props.modelValue,
  async (val) => {
    if (!val) return;
    resetView();
    if (!props.familyId) return;
    loading.value = true;
    try {
      const detail = await familyApi.get(props.familyId);
      console.log("Family Details:", detail);
      Object.assign(form, familyFormFromDetail(detail));
      students.value = detail.students || [];
      console.log("Family Student Details:", detail.students);
      meta.createdOnUtc = detail.createdOnUtc || null;
      meta.familyStatusName = detail.familyStatusName || "";
    } catch (err) {
      isOpen.value = false;
      notify.error(getApiErrorMessage(err));
    } finally {
      loading.value = false;
    }
  }
);
</script>
