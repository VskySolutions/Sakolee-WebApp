<template>
  <q-page padding>
    <!-- Create is retired here — students are created via Family registration (Quick Registration or
         the Family page), which mints the Person/login and its FamilyId link together. -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'All Student' }]"
      title="All Students"
      description="Centralized management for all active students."
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
      title="All Students"
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
      <div class="row q-col-gutter-lg">

        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          Identity
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">First Name</div>
          <div class="text-2e fs-14">{{ form.firstName || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Last Name</div>
          <div class="text-2e fs-14">{{ form.lastName || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Family Name</div>
          <div class="text-2e fs-14">{{ form.familyName || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500 q-mb-xs">
            Status
          </div>

          <q-badge :color="form.active ? 'positive' : 'grey'">
            {{ form.active ? "Active" : "Inactive" }}
          </q-badge>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Student Number</div>
          <div class="text-2e fs-14">{{ form.studentNumber || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Gender</div>
          <div class="text-2e fs-14">{{ form.gender || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Date of Birth</div>
          <div class="text-2e fs-14">{{ formatDate(form.birthDate) }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Admission Date</div>
          <div class="text-2e fs-14">{{ formatDate(form.admissionDate) }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Allow Text Messaging</div>
          <div class="text-2e fs-14">
            {{ form.allowTextMessaging ? "Yes" : "No" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Email</div>
          <div class="text-2e fs-14">{{ form.email || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Cell Phone</div>
          <div class="text-2e fs-14">{{ form.cellPhone || "—" }}</div>
        </div>

        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          School
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">School</div>
          <div class="text-2e fs-14">{{ form.school || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Grade Level</div>
          <div class="text-2e fs-14">{{ form.gradeLevel || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Transportation</div>
          <div class="text-2e fs-14">{{ form.transportation || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">T-Shirt Size</div>
          <div class="text-2e fs-14">{{ form.tShirtSize || "—" }}</div>
        </div>
        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          Fee
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Fee Amount</div>
          <div class="text-2e fs-14">{{ form.feeAmount || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Fee Expiry Date</div>
          <div class="text-2e fs-14">{{ formatDate(form.feeExpiryDate) }}</div>
        </div>

        <div class="col-12">
          <div class="text-86 fs-12 fw-500">Fee Note</div>
          <div class="text-2e fs-14">{{ form.feeNote || "—" }}</div>
        </div>
        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          Medical
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Primary Doctor</div>
          <div class="text-2e fs-14">{{ form.primaryDoctor || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Health Insurance Carrier</div>
          <div class="text-2e fs-14">{{ form.healthInsuranceCarrier || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Has Immunizations?</div>
          <div class="text-2e fs-14">{{ form.hasImmunizations || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Immunizations</div>
          <div class="text-2e fs-14">{{ form.immunizationNotes || "—" }}</div>
        </div>

        <div class="col-12">
          <div class="text-86 fs-12 fw-500">Medications</div>
          <div class="text-2e fs-14">{{ form.medications || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Disabilities</div>
          <div class="text-2e fs-14">{{ form.disabilitiesNotes || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Allergies</div>
          <div class="text-2e fs-14">{{ form.allergiesNotes || "—" }}</div>
        </div>

        <div class="col-12">
          <div class="text-86 fs-12 fw-500">Special Needs</div>
          <div class="text-2e fs-14">{{ form.specialNeeds || "—" }}</div>
        </div>
        <div class="col-12 text-subtitle2 text-grey-8 q-mt-sm">
          Notes
        </div>

        <div class="col-12">
          <div class="text-86 fs-12 fw-500">Skill Notes</div>
          <div class="text-2e fs-14">{{ form.skillNotes || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Text Opt-In</div>
          <div class="text-2e fs-14">{{ form.textOptIn || "—" }}</div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">Mass Email Opt-Out</div>
          <div class="text-2e fs-14">{{ form.massEmailOptOut || "—" }}</div>
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
