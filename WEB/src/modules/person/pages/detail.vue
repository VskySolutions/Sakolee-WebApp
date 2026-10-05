<template>
  <app-form-dialog
    v-model="isOpen"
    :title="personName"
    :subtitle="personSubtitle"
    :avatar-text="getInitials(personName)"
    size="lg"
    hide-save
    hide-footer
  >
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="32px" />
    </div>

    <div v-else class="view-student-scss">
      <!-- =====================================================
           SUMMARY CARDS
           ===================================================== -->
      <div class="student-summary row q-col-gutter-md q-mb-md">
        <div class="col-12 col-sm-4">
          <div class="summary-card">
            <div class="summary-card__label">ACCOUNT STATUS</div>
            <q-badge :color="isUser ? 'primary' : 'grey-4'" :text-color="isUser ? 'white' : 'grey-8'">
              {{ isUser ? "Linked user account" : "Not a user" }}
            </q-badge>
          </div>
        </div>

        <div class="col-12 col-sm-4">
          <div class="summary-card">
            <div class="summary-card__label">GENDER</div>
            <div class="summary-card__value">
              {{ form.gender || "—" }}
            </div>
          </div>
        </div>

        <div class="col-12 col-sm-4">
          <div class="summary-card">
            <div class="summary-card__label">EMPLOYEE CODE</div>
            <div class="summary-card__value font-mono">
              {{ form.employeeCode || "—" }}
            </div>
          </div>
        </div>
      </div>

      <!-- =====================================================
           PERSONAL INFORMATION + CONTACT DETAILS
           ===================================================== -->
      <div class="row q-col-gutter-md">
        <!-- Personal Details -->
        <div class="col-12 col-md-6">
          <section class="border-80 br-16 pa-20">
            <div class="info-card__header">
              <q-icon name="o_person_outline" class="fs-16 text-4d" />
              <span class="text-4d fw-700 fs-12 lh-16">PERSONAL DETAILS</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Full Name</span>
                <span class="fs-11 fw-600 text-2e">{{ personName }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Preferred Name</span>
                <span class="fs-11 fw-600 text-2e">{{ form.preferredName || "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Date of Birth</span>
                <span class="fs-11 fw-600 text-2e">{{ formatDate(form.dateOfBirth) }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Gender</span>
                <span class="fs-11 fw-600 text-2e">{{ form.gender || "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Display Name</span>
                <span class="fs-11 fw-600 text-2e">{{ form.displayName || "—" }}</span>
              </div>
            </div>
          </section>
        </div>

        <!-- Contact Details -->
        <div class="col-12 col-md-6">
          <section class="border-80 br-16 pa-20">
            <div class="info-card__header">
              <q-icon name="o_contacts" class="fs-16 text-4d" />
              <span class="text-4d fw-700 fs-12 lh-16">CONTACT DETAILS</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Primary Email</span>
                <span class="fs-11 fw-600 text-d4">{{ form.primaryEmail || "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Alternate Email</span>
                <span class="fs-11 fw-600 text-2e">{{ form.secondaryEmail || "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Phone Number</span>
                <span class="fs-11 fw-600 text-2e">{{ form.mobileNumber ? `${form.countryCode || ''} ${form.mobileNumber}` : "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Alternate Phone</span>
                <span class="fs-11 fw-600 text-2e">{{ form.alternateMobileNumber || "—" }}</span>
              </div>
              <div class="info-row">
                <span class="fs-11 fw-400 text-86"></span>
                <span class="fs-11 fw-600 text-2e"></span>
              </div>
            </div>
          </section>
        </div>
      </div>

      <!-- =====================================================
           CONVERT TO USER BOX (Orange Border & Rounded)
           ===================================================== -->
      <div class="mt-24" v-if="!isUser && canCreateUser">
        <div class="pa-20 br-16" style="border: 2px solid #f2c037; background-color: #fff8e1;">
          <div class="row items-center justify-between">
            <div class="row items-center q-gutter-x-md">
              <q-icon name="o_person_add" size="24px" color="orange-9" />
              <div>
                <div class="text-weight-bold text-orange-10 fs-12">Account Setup Required</div>
                <div class="text-caption text-grey-8">This person has no login account yet.</div>
              </div>
            </div>
            <q-btn unelevated color="orange-9" text-color="white" label="Convert to User" no-caps style="border-radius: 12px;" @click="convertToUser" />
          </div>
        </div>
      </div>

      <!-- =====================================================
           AUDIT
           ===================================================== -->
      <div class="mt-24">
        <app-record-audit :audit="audit" />
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>

import { ref, reactive, computed, watch } from "vue";
import { useRouter } from "vue-router";
import { personApi, getApiErrorMessage } from "services/api";
import { usePermissions, Permissions } from "composables/usePermissions";
import { useNotify } from "composables/useNotify";
import { useTenantOptions } from "composables/useTenantOptions";
import { useDateFormat } from "composables/useDateFormat";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppRecordAudit from "components/common/AppRecordAudit.vue";

// Props and emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  personId: { type: String, default: null }
});
const emit = defineEmits(["update:modelValue"]);

const router = useRouter();
const notify = useNotify();
const { formatDate } = useDateFormat();
const { has } = usePermissions();
const { canChooseTenant, loadTenants } = useTenantOptions();
const canCreateUser = computed(() => has(Permissions.UsersWrite));

const loading = ref(false);
const isUser = ref(false);
const personCode = ref("");
const audit = ref(null);

// Form data
const form = reactive({
  tenantIds: [],
  suffix: "",
  firstName: "",
  middleName: "",
  lastName: "",
  preferredName: "",
  displayName: "",
  gender: null,
  dateOfBirth: "",
  primaryEmail: "",
  secondaryEmail: "",
  mobileNumber: "",
  countryCode: null,
  alternateMobileNumber: "",
  employeeCode: ""
});

// Computed property for dialog open state
const isOpen = computed({
  get: () => props.modelValue,
  set: (v) => emit("update:modelValue", v)
});

// Computed properties for display
const personName = computed(() =>
  [form.firstName, form.middleName, form.lastName].filter(Boolean).join(" ") || form.displayName || "Person");

  // Computed property for subtitle
const personSubtitle = computed(() => {
  const code = personCode.value ? `#${personCode.value}` : "#—";
  const status = isUser.value ? "Linked user account" : "Not a user";
  return `Person ID: ${code} • ${status} • CRM Record`;
});

// Function to get initials for avatar
const getInitials = (name) => {
  if (!name || name === "Person") return "P";
  return name.split(" ").map(n => n[0]).join("").substring(0, 2).toUpperCase();
};

// Function to load person details
const load = async () => {
  if (!props.personId) return;
  loading.value = true;
  try {
    if (canChooseTenant.value) await loadTenants();
    const detail = await personApi.get(props.personId);
    const p = detail.profile || detail;
    isUser.value = detail.isUser || false;
    personCode.value = p.personCode || "";
    audit.value = p.audit || null;
    
    form.tenantIds = p.tenantIds || p.TenantIds || [];
    form.suffix = p.suffix || p.Suffix || "";
    form.firstName = p.firstName || p.FirstName || "";
    form.middleName = p.middleName || p.MiddleName || "";
    form.lastName = p.lastName || p.LastName || "";
    form.preferredName = p.preferredName || p.PreferredName || "";
    form.displayName = p.displayName || p.DisplayName || "";
    form.gender = p.gender || p.Gender || null;
    
    const dob = p.dateOfBirth || p.DateOfBirth;
    form.dateOfBirth = dob ? dob.substring(0, 10) : "";
    
    form.primaryEmail = p.primaryEmail || p.PrimaryEmail || "";
    form.secondaryEmail = p.secondaryEmail || p.SecondaryEmail || "";
    form.mobileNumber = p.mobileNumber || p.MobileNumber || "";
    form.countryCode = p.countryCode || p.CountryCode || null;
    form.alternateMobileNumber = p.alternateMobileNumber || p.AlternateMobileNumber || "";
    form.employeeCode = p.employeeCode || p.EmployeeCode || "";
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
};

// Watchers to handle prop changes and open/close state
watch(
  () => props.modelValue,
  (value) => {
    if (value) {
      load();
    }
  }
);

watch(
  () => props.personId,
  () => {
    if (props.modelValue) {
      load();
    }
  }
);


//
const convertToUser = () => {
  isOpen.value = false;
  router.push({ name: "users", query: { personId: props.personId } });
};
</script>