<template>
  <!-- Read-only View Family dialog (same app-form-dialog pattern as the Studio Locations view) -->
  <app-form-dialog
    v-model="isOpen"
    :title="form.familyName ? `View ${form.familyName}` : 'View Family'"
    size="lg"
    hide-footer
    @cancel="resetView"
  >
    <div class="family-view row q-col-gutter-lg">
      <q-inner-loading :showing="loading" />

      <!-- ================= Left column ================= -->
      <div class="col-12 col-md-7">
        <div class="fv-stack">
        <!-- Family Information -->
        <div class="fv-card">
          <div class="fv-card__title">
            <q-icon name="o_family_restroom" size="18px" color="primary" />
            Family Information
          </div>

          <div class="fv-label">Family Name</div>
          <div class="fv-value fs-18 fw-700">{{ form.familyName || "—" }}</div>

          <q-separator class="q-my-md" />

          <div class="row q-col-gutter-md">
            <div class="col-6">
              <div class="fv-label">Location</div>
              <span v-if="form.studioLocationName" class="fv-pill">{{ form.studioLocationName }}</span>
              <div v-else class="fv-value">—</div>
            </div>

            <div class="col-6">
              <div class="fv-label">Status</div>
              <span :class="form.active ? 'fv-pill' : 'fv-pill fv-pill--grey'">
                {{ form.active ? "Active" : "Inactive" }}
              </span>
            </div>

            <div class="col-6">
              <div class="fv-label">Registration Date</div>
              <div class="fv-value">{{ formatLongDate(meta.createdOnUtc) }}</div>
            </div>

            <div class="col-6">
              <div class="fv-label">Family Status</div>
              <div class="fv-value">{{ familyStatusLabel }}</div>
            </div>
          </div>
        </div>

        <!-- Enrolled Students -->
        <div class="fv-card">
          <div class="fv-card__title">
            <q-icon name="o_school" size="18px" color="primary" />
            Enrolled Students
            <q-space />
            <span class="fv-count">{{ activeStudentCount }} Active</span>
          </div>

          <template v-if="students.length">
            <div class="fv-table">
              <div class="fv-table__row fv-table__head">
                <div>Student Name</div>
                <div>Student #</div>
                <div>Status</div>
              </div>

              <div
                v-for="student in students"
                :key="student.studentId"
                class="fv-table__row"
              >
                <div class="row items-center no-wrap q-gutter-x-sm">
                  <span class="fv-avatar">{{ initials(student.firstName, student.lastName) }}</span>
                  <span class="fw-600 text-2e ellipsis">{{ studentName(student) }}</span>
                </div>
                <div class="text-86">{{ student.studentNumber || "—" }}</div>
                <div>
                  <span :class="student.active ? 'fv-dot' : 'fv-dot fv-dot--grey'">
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
                <q-icon name="o_group" size="18px" color="primary" />
                Secondary Contact
              </div>

              <template v-if="form.secondaryContact">
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
              </template>

              <div v-else class="fv-empty">No secondary contact.</div>
            </div>
          </div>

          <div>
            <div class="fv-card full-height">
              <div class="fv-card__title">
                <q-icon name="o_campaign" size="18px" color="primary" />
                Referral Details
              </div>

              <div class="fv-row">
                <span class="fv-label q-mb-none">Source</span>
                <span class="fv-value fw-700 text-right">{{ form.source || "—" }}</span>
              </div>
              <div class="fv-row">
                <span class="fv-label q-mb-none">Referral</span>
                <span class="fv-value fw-700 text-right">{{ form.referralName || "—" }}</span>
              </div>

              <q-separator class="q-my-sm" />

              <div class="fv-label">Health Insurance</div>
              <div class="fv-value">{{ form.healthInsuranceCarrier || "—" }}</div>
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
            Primary Contact
          </div>

          <div class="row q-col-gutter-sm">
            <div class="col-6">
              <div class="fv-label">First Name</div>
              <div class="fv-value fw-700">{{ form.firstName || "—" }}</div>
            </div>
            <div class="col-6">
              <div class="fv-label">Last Name</div>
              <div class="fv-value fw-700">{{ form.lastName || "—" }}</div>
            </div>
            <div class="col-12">
              <div class="fv-label">Relationship</div>
              <div class="fv-value fw-700">{{ form.relation || "—" }}</div>
            </div>
          </div>

          <q-separator class="q-my-md" />

          <div class="fv-card__title q-mb-sm">
            Contact Info
            <q-space />
            <span v-if="form.isBillingContact" class="fv-count">Billing Contact</span>
          </div>

          <div class="fv-line">
            <q-icon name="o_mail" size="16px" color="primary" />
            <span class="ellipsis" :title="form.email">{{ form.email || "—" }}</span>
          </div>
          <div v-for="phone in primaryPhones" :key="phone.label" class="fv-line">
            <q-icon :name="phone.icon" size="16px" color="primary" />
            {{ phone.value }}
            <span class="text-86 fs-12">({{ phone.label }})</span>
          </div>
          <div class="fv-line items-start">
            <q-icon name="o_place" size="16px" color="primary" class="q-mt-xs" />
            <div class="fs-13">
              <template v-if="addressLines.length">
                <div v-for="line in addressLines" :key="line">{{ line }}</div>
              </template>
              <template v-else>—</template>
            </div>
          </div>

          <div v-if="form.isAuthorizedToPickUpStudent" class="q-mt-sm">
            <span class="fv-chip">
              <q-icon name="o_check_circle" size="13px" /> Authorized Pick Up
            </span>
          </div>
        </div>

        <!-- Emergency Contact -->
        <div class="fv-card fv-card--danger">
          <div class="fv-card__title text-negative">
            <q-icon name="o_local_hospital" size="18px" color="negative" />
            Emergency Contact
          </div>

          <div class="fv-value fw-700">{{ form.emergencyContactPerson || "—" }}</div>

          <div class="fv-line q-mt-xs">
            <q-icon name="o_call" size="16px" color="primary" />
            {{ form.emergencyPhone || "—" }}
          </div>
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

const familyStatusLabel = computed(() =>
  meta.familyStatusName ||
  props.familyStatusOptions.find((x) => x.value === form.familyStatusId)?.label ||
  "—"
);

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

const studentName = (s) =>
  s.firstName || s.lastName ? fullName(s.firstName, s.lastName) : s.studentNumber || "Student";

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
      Object.assign(form, familyFormFromDetail(detail));
      students.value = detail.students || [];
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

<style scoped lang="scss">
.family-view {
  position: relative;
  min-height: 200px;
}

// Vertical card stacks and the two-card row use CSS gap — nesting Quasar gutters here
// collapses the spacing between cards.
.fv-stack {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.fv-pair {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 24px;

  @media (max-width: 599px) {
    grid-template-columns: 1fr;
  }
}

.fv-card {
  min-width: 0;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  padding: 20px 24px;

  &--danger {
    border-left: 4px solid #ba1a1a;
  }

  &__title {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 16px;
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 0.04em;
    text-transform: uppercase;
    color: #6b7280;
  }
}

.fv-label {
  font-size: 12px;
  color: #6b7280;
  margin-bottom: 4px;
}

.fv-value {
  font-size: 14px;
  color: #131b2e;
  word-break: break-word;
}

.fv-pill {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  color: #4648d4;
  background: #eef2ff;

  &--grey {
    color: #4b5563;
    background: #f3f4f6;
  }
}

.fv-count {
  padding: 2px 8px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0;
  text-transform: none;
  color: #fff;
  background: #4648d4;
}

.fv-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
  color: #4648d4;
  background: #eef2ff;
  border: 1px solid #c7d2fe;
}

.fv-line {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
  color: #131b2e;
  margin-bottom: 8px;
  word-break: break-word;

  > .ellipsis {
    min-width: 0;
  }
}

.fv-row {
  display: flex;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 8px;
}

.fv-table {
  border-top: 1px solid #e5e7eb;

  &__row {
    display: grid;
    grid-template-columns: 2fr 1fr 1fr;
    align-items: center;
    gap: 8px;
    padding: 12px 0;
    font-size: 14px;
  }

  &__head {
    font-size: 11px;
    font-weight: 700;
    text-transform: uppercase;
    color: #374151;
  }
}

.fv-avatar {
  flex-shrink: 0;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  color: #4648d4;
  background: #eef2ff;
}

.fv-dot {
  font-weight: 600;
  color: #4648d4;

  &::before {
    content: "";
    display: inline-block;
    width: 6px;
    height: 6px;
    margin-right: 6px;
    border-radius: 50%;
    background: currentColor;
    vertical-align: middle;
  }

  &--grey {
    color: #9ca3af;
  }
}

.fv-empty {
  font-size: 13px;
  color: #9ca3af;
}
</style>
