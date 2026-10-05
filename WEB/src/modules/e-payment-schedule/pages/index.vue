<template>
  <q-page padding>
    <!-- Page header with breadcrumbs, search, and create button -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'E-Payment Schedule' }
      ]"
      title="E-Payment Schedule"
      description="Manage your E-Payment Schedule here."
      :search="search"
      show-search
      search-placeholder="Search E-Payment Schedule name"
     
      :show-add="canWrite"
      add-label="Create E-Payment Schedule"
      show-back
      @update:search="search = $event"
  
      @add="openCreate"
      @back="$router.back()"
    />
    <!--Hiding the filter attribute-->
    <!-- show-filters
      :filter-count="filterChips.length"
      @filters="filterOpen = true" -->
    <!-- Filter drawer for filtering E-Payment Schedule records -->
    <!-- <app-filter-drawer
      v-model="filterOpen"
      :chips="filterChips"
      @remove="removeFilter"
      @clear="clearFilters"
    >
      <app-column-filters
        v-model="filters"
        :columns="filterableColumns"
      />
      <q-toggle
        v-if="canManageDeleted"
        v-model="showDeleted"
        label="Show deleted?"
        dense
        class="q-mt-md"
      />
    </app-filter-drawer> -->
    <!-- Table displaying all E-Payment Schedule records -->
    <app-data-table
      page-key="e-payment-schedule"
      row-key="ePaymentScheduleId"
      title="E-Payment Schedule"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Active Status -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-toggle
            :model-value="cell.row.active"
            :disable="!canWrite"
            color="positive"
            @update:model-value="
              toggleActive(cell.row, $event)
            "
          >
            <q-tooltip>
              {{ cell.row.active ? "Active" : "Inactive" }}
            </q-tooltip>
          </q-toggle>
        </q-td>
      </template>
      <!-- Action buttons for viewing, editing, and deleting -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <!-- View button -->
          <q-btn
            v-if="canRead"
            type="a"
            flat
            round
            dense
            color="primary"
            icon="o_visibility"
            @click="openView(cell.row)"
          >
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <!-- Edit button -->
          <q-btn
            v-if="canWrite"
            type="a"
            flat
            round
            dense
            color="primary"
            icon="o_edit"
            @click="openEdit(cell.row)"
          >
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <!-- Delete button -->
          <q-btn
            v-if="canDelete"
            type="a"
            flat
            round
            dense
            color="negative"
            icon="o_delete"
            @click="deleteEPaymentSchedule(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <!-- Dialog used for creating and editing E-Payment Schedule -->
    <create-edit
      v-model="formOpen"
      :editing="editing"
      :e-payment-schedule="selectedEPaymentSchedule"
      @saved="load"
    />
    <!-- View E-Payment Schedule -->
    <app-form-dialog
      v-model="viewOpen"
      title="View E-Payment Schedule"
      size="sm"
      hide-save
      @cancel="closeView"
    >
      <div class="row q-col-gutter-lg">
        <!-- Name -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Name
          </div>
          <div class="text-2e fs-14">
            {{ viewEPaymentSchedule.name || "—" }}
          </div>
        </div>
        <!-- Status -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge
            :class="
              viewEPaymentSchedule.active
                ? 'active-badge'
                : 'inactive-badge'
            "
          >
            {{
              viewEPaymentSchedule.active
                ? "Active"
                : "Inactive"
            }}
          </q-badge>
        </div>
        <!-- Tenant -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Tenant
          </div>
          <div class="text-2e fs-14">
            {{ viewEPaymentSchedule.tenantName || "—" }}
          </div>
        </div>
        <!-- Created By -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewEPaymentSchedule.createdBy || "—" }}
          </div>
        </div>
        <!-- Created On -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{
              formatDateTime(
                viewEPaymentSchedule.createdOnUtc
              )
            }}
          </div>
        </div>
        <!-- Updated By -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewEPaymentSchedule.updatedBy || "—" }}
          </div>
        </div>
        <!-- Updated On -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{
              viewEPaymentSchedule.updatedBy ? formatDateTime(viewEPaymentSchedule.updatedOnUtc): "—"
            }}
          </div>
        </div>
      </div>
    </app-form-dialog>
  </q-page>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import { date, debounce } from "quasar";
import { ePaymentScheduleApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { usePermissions } from "composables/usePermissions";
import { useListTable } from "composables/useListTable";
import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFormDialog from "components/common/AppFormDialog.vue";
import CreateEdit from "modules/e-payment-schedule/components/CreateEdit.vue";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
const { showDeleted, canManageDeleted } = useDeletedRecords();

// Check permissions for E-Payment Schedule actions.
const canRead = computed(() =>
  has("ePaymentSchedules.read")
);
// Check permissions for E-Payment Schedule actions.
const canWrite = computed(() =>
  has("ePaymentSchedules.write")
);
// Check permissions for E-Payment Schedule actions.
const canDelete = computed(() =>
  has("ePaymentSchedules.delete")
);
// Format UTC date values for display.
const formatDateTime = (value) => {
  if (!value) {
    return "—";
  }
  // Add UTC indicator when the API date does not include
  // timezone information.
  const iso = /(Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : `${value}Z`;
  return date.formatDate(new Date(iso), "MM/DD/YYYY hh:mm A");
};
// Define the columns displayed in the E-Payment Schedule table.
const columns = [
  {
    name: "name",
    label: "Name",
    field: "name",
    align: "left",
    sortable: true,
    default: true,
    filterable: true
  },
  {
    name: "tenantName",
    label: "Tenant",
    field: "tenantName",
    align: "left",
    sortable: true,
    filterable: false,
    default: true
  },
  {
    name: "createdBy",
    label: "Created By",
    field: "createdBy",
    align: "left",
    sortable: true,
    default: true,
    filterable: false,
    format: (val) => val || "—"
  },
  {
    name: "createdOnUtc",
    label: "Created On",
    field: "createdOnUtc",
    align: "left",
    sortable: true,
    default: true,
    filterable: false,
    format: (val) => formatDateTime(val)
  },
  {
    name: "updatedBy",
    label: "Updated By",
    field: "updatedBy",
    align: "left",
    sortable: true,
    default: true,
    filterable: false,
    format: (val) => val || "—"
  },
  {
    name: "updatedOnUtc",
    label: "Updated On",
    field: "updatedOnUtc",
    align: "left",
    sortable: true,
    default: true,
    filterable: false,
    format: (val, row) =>
      row?.updatedBy
        ? formatDateTime(val)
        : "—"
  },
  {
    name: "active",
    label: "Status",
    field: "active",
    align: "left",
    sortable: true,
    default: true,
    filterable: true,
    filterOptions: [
      {
        label: "Active",
        value: true
      },
      {
        label: "Inactive",
        value: false
      }
    ]
  },
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "left"
  }
];

const filterOpen = ref(false);
// Configure the reusable list table.
const {
  rows,
  loading,
  totalRecords,
  search,
  pagination,
  load,
  onRequest
} = useListTable({
  pageKey: "e-payment-schedule",
  // Fetch E-Payment Schedule records from the API
  // with optional search and sorting.
  fetcher: ({ page, limit, sortBy, descending }) => ePaymentScheduleApi.list({page, limit, search:search.value || undefined, sortBy, descending, name:filters.name || undefined, active:filters.active ?? undefined,showDeleted:showDeleted.value})
    .then((response) => ({ data: response?.data || [], total: response?.meta?.totalRecords || 0 })),
  // Handle API errors.
  onError: (error) => {
    notify.error(getApiErrorMessage(error, "Unable to load E-Payment Schedule records."));
  }
});

// Configure server-side filters.
const {
  filters,
  filterableColumns,
  filterChips,
  removeFilter,
  clearFilters
} = useColumnFilters(columns, rows, { server: true });

// Reload the table when the search value changes.
// Debounce prevents an API call for every keystroke.
const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch([search, showDeleted, filters], reload, { deep: true });
// Create/Edit dialog state.
const formOpen = ref(false);
const editing = ref(false);
const selectedEPaymentSchedule = ref(null);
// View dialog state.
const viewOpen = ref(false);
const viewLoading = ref(false);
// Define the structure of the E-Payment Schedule record being viewed.
const viewEPaymentSchedule = ref({
  ePaymentScheduleId: null,
  name: "",
  tenantName: "",
  createdBy: null,
  createdOnUtc: null,
  updatedBy: null,
  updatedOnUtc: null,
  active: true,
  deleted: false
});

// Reset the view state.
const resetViewEPaymentSchedule = () => {
  viewEPaymentSchedule.value = {
    ePaymentScheduleId: null,
    name: "",
    tenantName: "",
    createdBy: null,
    createdOnUtc: null,
    updatedBy: null,
    updatedOnUtc: null,
    active: true,
    deleted: false
  };
};
// Open the View dialog for the selected record.
const openView = async (row) => {
  resetViewEPaymentSchedule();
  viewOpen.value = true;
  viewLoading.value = true;
  // Fetch the latest record from the API.
  try {
    const ePaymentSchedule =
      await ePaymentScheduleApi.get(
        row.ePaymentScheduleId
      );
    viewEPaymentSchedule.value = {
      ePaymentScheduleId: ePaymentSchedule?.ePaymentScheduleId,
      name: ePaymentSchedule?.name || "",
      tenantName: ePaymentSchedule?.tenantName || "",
      createdBy: ePaymentSchedule?.createdBy || "",
      createdOnUtc: ePaymentSchedule?.createdOnUtc || null,
      updatedBy: ePaymentSchedule?.updatedBy || "",
      updatedOnUtc: ePaymentSchedule?.updatedOnUtc || null,
      active: ePaymentSchedule?.active ?? true,
      deleted: ePaymentSchedule?.deleted ?? false
    };
  } catch (error) {
    viewOpen.value = false;
    notify.error(
      getApiErrorMessage(error, "Unable to load E-Payment Schedule record."));
  } finally {
    viewLoading.value = false;
  }
};

// Close the View dialog and reset its state.
const closeView = () => {
  viewOpen.value = false;
  resetViewEPaymentSchedule();
};

// Open the form for creating a new record.
const openCreate = () => {
  selectedEPaymentSchedule.value = null;
  editing.value = false;
  formOpen.value = true;
};

// Load the selected record and open the edit form.
const openEdit = async (row) => {
  try {
    const ePaymentSchedule =
      await ePaymentScheduleApi.get(row.ePaymentScheduleId);
    selectedEPaymentSchedule.value = ePaymentSchedule;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    notify.error(getApiErrorMessage(error, "Unable to load E-Payment Schedule record."));
  }
};

// Toggle Active / Inactive status.
const toggleActive = async (row, active) => {
  const previousValue = row.active;
  // Update UI immediately.
  row.active = active;
  try {
    await ePaymentScheduleApi.update(row.ePaymentScheduleId, { name: row.name, active });
    notify.success(active ? "E-Payment Schedule activated." : "E-Payment Schedule deactivated.");
  } catch (error) {
    // Restore previous value if update fails.
    row.active = previousValue;
    notify.error(getApiErrorMessage(error, "Unable to update E-Payment Schedule status."));
  }
};

// Delete the selected E-Payment Schedule record.
const deleteEPaymentSchedule = async (row) => {
  const confirmed = await confirm({
    title: "Delete E-Payment Schedule",
    message: `Delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  // Stop if the user cancels the operation.
  if (!confirmed) {
    return;
  }
  try {
    await ePaymentScheduleApi.remove(row.ePaymentScheduleId);
    notify.success("E-Payment Schedule deleted.");
    load();
  } catch (error) {
    notify.error(getApiErrorMessage(error, "Unable to delete E-Payment Schedule record.")
    );
  }
};
// Load E-Payment Schedule records when the page opens.
load();
</script>
