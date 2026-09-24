<template>
  <q-page padding>
    <!-- Create is retired here — students are created via Family registration (Quick Registration or
         the Family page), which mints the Person/login and its FamilyId link together. -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' }, { label: 'Student' }]"
      :search="search"
      show-search
      search-placeholder="Search name or email"
      show-filters
      :filter-count="filterChips.length"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
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
          <!-- Edit is retired here too — see the header note above. -->
          <q-btn v-if="canDelete" type="a" flat round dense color="negative" icon="o_delete" @click="remove(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- View drawer — read-only display, not a disabled form: this page no longer creates or edits
         students, so nothing here is ever typed into. -->
    <app-form-drawer v-model="formOpen" title="View Student" hide-save @cancel="resetForm">
      <div class="row q-col-gutter-md">
        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Identity</div>
        <app-readonly-field :model-value="form.firstName" label="First Name" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.lastName" label="Last Name" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.familyName" label="Family Name" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.studentNumber" label="Student Number" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.gender" label="Gender" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="formatDate(form.birthDate)" label="Date of Birth" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="formatDate(form.admissionDate)" label="Admission Date" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.allowTextMessaging ? 'Yes' : 'No'" label="Allow Text Messaging" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.email" label="Email" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.cellPhone" label="Cell Phone" class="col-12 col-sm-6" />

        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">School</div>
        <app-readonly-field :model-value="form.school" label="School" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.gradeLevel" label="Grade Level" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.transportation" label="Transportation" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.tShirtSize" label="T-Shirt Size" class="col-12 col-sm-6" />

        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Fee</div>
        <app-readonly-field :model-value="form.feeAmount" label="Fee Amount" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="formatDate(form.feeExpiryDate)" label="Fee Expiry Date" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.feeNote" label="Fee Note" class="col-12" />

        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Medical</div>
        <app-readonly-field :model-value="form.primaryDoctor" label="Primary Doctor" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.healthInsuranceCarrier" label="Health Insurance Carrier" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.hasImmunizations" label="Has Immunizations?" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.immunizationNotes" label="Immunizations" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.medications" label="Medications" class="col-12" />
        <app-readonly-field :model-value="form.disabilitiesNotes" label="Disabilities" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.allergiesNotes" label="Allergies" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.specialNeeds" label="Special Needs" class="col-12" />

        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">Notes</div>
        <app-readonly-field :model-value="form.skillNotes" label="Skill Notes" class="col-12" />
        <app-readonly-field :model-value="form.textOptIn" label="Text Opt-In" class="col-12 col-sm-6" />
        <app-readonly-field :model-value="form.massEmailOptOut" label="Mass Email Opt-Out" class="col-12 col-sm-6" />

        <div class="col-12 q-mt-sm">
          <app-field-label label="Status" />
          <q-badge :color="form.active ? 'positive' : 'grey'">{{ form.active ? "Active" : "Inactive" }}</q-badge>
        </div>
      </div>
    </app-form-drawer>
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { debounce } from "quasar";
import { studentApi, getApiErrorMessage } from "services/api";
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
import AppReadonlyField from "components/common/AppReadonlyField.vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const auditColumns = useAuditColumns();
const { formatDate } = useDateFormat();
const { has } = usePermissions();
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

// ---- View ----
const formOpen = ref(false);
const blankForm = () => ({
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
};

const openView = (row) => {
  resetForm();
  Object.assign(form, {
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
  formOpen.value = true;
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
