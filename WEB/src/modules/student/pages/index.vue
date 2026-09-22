<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' }, { label: 'Student' }]"
      :search="search"
      show-search
      search-placeholder="Search name or email"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canWrite"
      add-label="Create Student"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-select v-model="filters.active" :options="activeFilterOptions" label="Status" />
    </app-filter-drawer>

    <app-data-table
      page-key="students"
      row-key="studentId"
      title="All students"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">{{ cell.value ? "Active" : "Inactive" }}</q-badge>
        </q-td>
      </template>

      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn type="a" flat round dense color="primary" icon="o_visibility" @click="openView(cell.row)">
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <q-btn v-if="canWrite" type="a" flat round dense color="primary" icon="o_edit" @click="openEdit(cell.row)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn v-if="canDelete" type="a" flat round dense color="negative" icon="o_delete" @click="remove(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit / View drawer -->
    <app-form-drawer
      v-model="formOpen"
      :title="viewing ? 'View Student' : editing ? 'Edit Student' : 'Create Student'"
      :saving="saving"
      :hide-save="viewing"
      @submit="submitForm"
      @cancel="resetForm"
    >
      <q-form ref="formRef" greedy>
        <div class="row q-col-gutter-md">
          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Identity</div>
          <app-text-field v-model="form.firstName" label="First Name" required :disable="viewing" class="col-12 col-sm-6" :rules="[(v) => !!v || 'First name is required']" />
          <app-text-field v-model="form.lastName" label="Last Name" required :disable="viewing" class="col-12 col-sm-6" :rules="[(v) => !!v || 'Last name is required']" />
          <app-text-field v-model="form.familyName" label="Family Name" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.studentNumber" label="Student Number" :disable="viewing" class="col-12 col-sm-6" />
          <app-select v-model="form.gender" label="Gender" :options="genderOptions" :disable="viewing" class="col-12 col-sm-6" />
          <app-date-field v-model="form.birthDate" label="Date of Birth" required :disable="viewing" class="col-12 col-sm-6" :rules="[(v) => !!v || 'Date of birth is required']" />
          <app-date-field v-model="form.admissionDate" label="Admission Date" :disable="viewing" class="col-12 col-sm-6" />
          <div class="col-12 col-sm-6 toggle-row-inline"><q-toggle v-model="form.allowTextMessaging" color="primary" :disable="viewing" /><span class="q-ml-sm">Allow text messaging</span></div>
          <app-text-field
            v-model="form.email" label="Email" required :disable="viewing" class="col-12 col-sm-6"
            :error="!!emailError" :error-message="emailError"
            :rules="[(v) => !!v || 'Email is required']"
            hint="A login account is created for the student with this email."
          />
          <app-text-field v-model="form.cellPhone" label="Cell Phone" :disable="viewing" class="col-12 col-sm-6" />

          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">School</div>
          <app-text-field v-model="form.school" label="School" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.gradeLevel" label="Grade Level" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.transportation" label="Transportation" placeholder="e.g. School Bus" :disable="viewing" class="col-12 col-sm-6" />
          <app-select v-model="form.tShirtSize" label="T-Shirt Size" :options="tShirtSizeOptions" :disable="viewing" class="col-12 col-sm-6" />

          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Fee</div>
          <app-text-field v-model.number="form.feeAmount" label="Fee Amount" type="number" :disable="viewing" class="col-12 col-sm-6" />
          <app-date-field v-model="form.feeExpiryDate" label="Fee Expiry Date" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.feeNote" label="Fee Note" :disable="viewing" class="col-12" />

          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Medical</div>
          <app-text-field v-model="form.primaryDoctor" label="Primary Doctor" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.healthInsuranceCarrier" label="Health Insurance Carrier" :disable="viewing" class="col-12 col-sm-6" />
          <app-select v-model="form.hasImmunizations" label="Has Immunizations?" :options="hasImmunizationsOptions" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.immunizationNotes" label="Immunizations" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.medications" label="Medications" type="textarea" :disable="viewing" class="col-12" />
          <app-text-field v-model="form.disabilitiesNotes" label="Disabilities" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.allergiesNotes" label="Allergies" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.specialNeeds" label="Special Needs" type="textarea" :disable="viewing" class="col-12" />

          <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Notes</div>
          <app-text-field v-model="form.skillNotes" label="Skill Notes" type="textarea" :disable="viewing" class="col-12" />
          <app-text-field v-model="form.textOptIn" label="Text Opt-In" :disable="viewing" class="col-12 col-sm-6" />
          <app-text-field v-model="form.massEmailOptOut" label="Mass Email Opt-Out" :disable="viewing" class="col-12 col-sm-6" />

          <div v-if="editing || viewing" class="col-12">
            <q-toggle v-model="form.active" label="Active" :disable="viewing" />
          </div>
        </div>
      </q-form>
    </app-form-drawer>

    <temp-password-dialog v-model="tempPwOpen" :password="tempPassword" />
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { debounce } from "quasar";
import { studentApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useAuditColumns } from "composables/useAuditColumns";
import { useDateFormat } from "composables/useDateFormat";
import { usePermissions, Permissions } from "composables/usePermissions";

import AppDataTable from "components/common/AppDataTable.vue";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppDateField from "components/common/AppDateField.vue";
import TempPasswordDialog from "components/temp_password_dialog.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const auditColumns = useAuditColumns();
const { formatDate } = useDateFormat();
const { has } = usePermissions();
const canWrite = computed(() => has(Permissions.StudentsWrite));
const canDelete = computed(() => has(Permissions.StudentsDelete));

// firstName/lastName/email are joined in from the linked Person and are not sortable server-side
// (StudentsController's SortMap can't reach outside the Student row — see its remarks).
const columns = [
  { name: "firstName", label: "First Name", field: "firstName", align: "left", default: true },
  { name: "lastName", label: "Last Name", field: "lastName", align: "left", default: true },
  { name: "email", label: "Email", field: (row) => row.email || "—", align: "left", default: true },
  { name: "cellPhone", label: "Cell Phone", field: (row) => row.cellPhone || "—", align: "left" },
  { name: "school", label: "School", field: (row) => row.school || "—", align: "left", sortable: true },
  { name: "gradeLevel", label: "Grade", field: (row) => row.gradeLevel || "—", align: "left" },
  { name: "admissionDate", label: "Admission Date", field: (row) => formatDate(row.admissionDate), sort: (row) => row.admissionDate || "", align: "left", sortable: true },
  { name: "active", label: "Status", field: "active", align: "left", sortable: true, default: true },
  ...auditColumns(),
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

const filters = reactive({ active: null });
const { rows, loading, totalRecords, search, filterOpen, pagination, load, onRequest } = useListTable({
  pageKey: "students",
  fetcher: ({ page, limit, sortBy, descending }) =>
    studentApi.list({
      page,
      limit,
      sortBy,
      descending,
      active: filters.active === null ? undefined : filters.active,
      search: search.value || undefined
    })
      .then((r) => ({ data: r?.data, total: r?.meta?.totalRecords })),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

const reload = debounce(() => { pagination.value.page = 1; load(); }, 300);
watch([search, filters], reload, { deep: true });

const activeFilterOptions = [{ label: "Active", value: true }, { label: "Inactive", value: false }];

const filterChips = computed(() => {
  const chips = [];
  if (filters.active !== null) chips.push({ key: "active", label: `Status: ${filters.active ? "Active" : "Inactive"}` });
  return chips;
});

const removeFilter = (key) => {
  if (key === "active") filters.active = null;
};
const clearFilters = () => {
  filters.active = null;
};

const genderOptions = ["Female", "Male", "Non-Binary", "Prefer not to say"];
const tShirtSizeOptions = ["Child XS", "Child S", "Child M", "Child L", "Adult S", "Adult M", "Adult L"];
const hasImmunizationsOptions = ["Yes", "No", "Exempt"];

// ---- Create / Edit ----
// Creating a student mints its own CRM Person (and login account, with the Student role) from the
// identity fields below — there is no existing Person to pick, unlike the old linked-Person flow.
const formOpen = ref(false);
const editing = ref(false);
const viewing = ref(false);
const saving = ref(false);
const emailError = ref("");
const formRef = ref(null);
const tempPwOpen = ref(false);
const tempPassword = ref("");
const blankForm = () => ({
  studentId: null,
  firstName: "",
  lastName: "",
  familyName: "",
  studentNumber: "",
  admissionDate: "",
  birthDate: "",
  gender: "",
  allowTextMessaging: true,
  email: "",
  cellPhone: "",
  school: "",
  gradeLevel: "",
  transportation: "",
  tShirtSize: "",
  feeAmount: null,
  feeExpiryDate: "",
  feeNote: "",
  primaryDoctor: "",
  healthInsuranceCarrier: "",
  hasImmunizations: "Yes",
  medications: "",
  immunizationNotes: "",
  disabilitiesNotes: "",
  allergiesNotes: "",
  specialNeeds: "",
  skillNotes: "",
  textOptIn: "",
  massEmailOptOut: "",
  active: true
});
const form = reactive(blankForm());

const resetForm = () => {
  Object.assign(form, blankForm());
  emailError.value = "";
  editing.value = false;
  viewing.value = false;
};

const openCreate = () => {
  resetForm();
  formOpen.value = true;
};

const populateFrom = (row) => {
  Object.assign(form, {
    studentId: row.studentId,
    firstName: row.firstName || "",
    lastName: row.lastName || "",
    familyName: row.familyName || "",
    studentNumber: row.studentNumber || "",
    admissionDate: row.admissionDate ? row.admissionDate.substring(0, 10) : "",
    birthDate: row.birthDate ? row.birthDate.substring(0, 10) : "",
    gender: row.gender || "",
    allowTextMessaging: row.allowTextMessaging ?? true,
    email: row.email || "",
    cellPhone: row.cellPhone || "",
    school: row.school || "",
    gradeLevel: row.gradeLevel || "",
    transportation: row.transportation || "",
    tShirtSize: row.tShirtSize || "",
    feeAmount: row.feeAmount ?? null,
    feeExpiryDate: row.feeExpiryDate ? row.feeExpiryDate.substring(0, 10) : "",
    feeNote: row.feeNote || "",
    primaryDoctor: row.primaryDoctor || "",
    healthInsuranceCarrier: row.healthInsuranceCarrier || "",
    hasImmunizations: row.hasImmunizations || "Yes",
    medications: row.medications || "",
    immunizationNotes: row.immunizationNotes || "",
    disabilitiesNotes: row.disabilitiesNotes || "",
    allergiesNotes: row.allergiesNotes || "",
    specialNeeds: row.specialNeeds || "",
    skillNotes: row.skillNotes || "",
    textOptIn: row.textOptIn || "",
    massEmailOptOut: row.massEmailOptOut || "",
    active: row.active
  });
};

const openEdit = (row) => {
  resetForm();
  editing.value = true;
  populateFrom(row);
  formOpen.value = true;
};

const openView = (row) => {
  resetForm();
  viewing.value = true;
  populateFrom(row);
  formOpen.value = true;
};

const submitForm = async ({ clearDraft } = {}) => {
  emailError.value = "";
  const valid = await formRef.value?.validate();
  if (!valid) return;

  const payload = {
    firstName: form.firstName,
    lastName: form.lastName,
    familyName: form.familyName || null,
    studentNumber: form.studentNumber || null,
    admissionDate: form.admissionDate || null,
    birthDate: form.birthDate || null,
    gender: form.gender || null,
    allowTextMessaging: form.allowTextMessaging,
    email: form.email || null,
    cellPhone: form.cellPhone || null,
    school: form.school || null,
    gradeLevel: form.gradeLevel || null,
    transportation: form.transportation || null,
    tShirtSize: form.tShirtSize || null,
    feeAmount: form.feeAmount,
    feeExpiryDate: form.feeExpiryDate || null,
    feeNote: form.feeNote || null,
    primaryDoctor: form.primaryDoctor || null,
    healthInsuranceCarrier: form.healthInsuranceCarrier || null,
    hasImmunizations: form.hasImmunizations || null,
    medications: form.medications || null,
    immunizationNotes: form.immunizationNotes || null,
    disabilitiesNotes: form.disabilitiesNotes || null,
    allergiesNotes: form.allergiesNotes || null,
    specialNeeds: form.specialNeeds || null,
    skillNotes: form.skillNotes || null,
    textOptIn: form.textOptIn || null,
    massEmailOptOut: form.massEmailOptOut || null
  };

  saving.value = true;
  try {
    if (editing.value) {
      await studentApi.update(form.studentId, { ...payload, active: form.active });
      notify.success("Student updated.");
    } else {
      const created = await studentApi.create(payload);
      notify.success("Student created.");
      // A login account (with the Student role) is minted alongside the student — its temporary
      // password is only ever shown here.
      if (created?.temporaryPassword) {
        tempPassword.value = created.temporaryPassword;
        tempPwOpen.value = true;
      }
    }
    clearDraft?.();
    formOpen.value = false;
    resetForm();
    load();
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      emailError.value = "A student with this email already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};

const remove = async (row) => {
  const ok = await confirm({
    title: "Delete student",
    message: `Delete "${row.firstName} ${row.lastName}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;
  try {
    await studentApi.remove(row.studentId);
    notify.success("Student deleted.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>

<style scoped>
.toggle-row-inline {
  display: flex;
  align-items: center;
  min-height: 40px;
}
</style>
