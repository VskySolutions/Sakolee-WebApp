<template>
  <q-page padding>
    <!-- Page header with breadcrumbs, search, and create button -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Billing Cycles' }
      ]"
      title="Billing Cycles"
      description="Manage your billing cycles here."
      :search="search"
      show-search
      search-placeholder="Search billing cycle name"
     
      :show-add="canWrite"
      add-label="Create Billing Cycle"
      show-back
      @update:search="search = $event"
     
      @add="openCreate"
      @back="$router.back()"
    />
    <!--Hiding the filter attribute-->
    <!-- show-filters
      :filter-count="filterChips.length"
      @filters="filterOpen = true" -->
    <!-- Filter drawer -->
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
    <!-- Table displaying all Billing Cycles -->
    <app-data-table
      page-key="billing-cycles"
      row-key="billingCycleId"
      title="Billing Cycles"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Active toggle column -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-toggle
            :model-value="cell.row.active"
            :disable="!canWrite"
            color="positive"
            @update:model-value="toggleActive(cell.row, $event)"
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
            @click="deleteBillingCycle(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <!-- Drawer used to create or edit a Billing Cycle -->
    <create-edit
      v-model="formOpen"
      :editing="editing"
      :billing-cycle="selectedBillingCycle"
      @saved="load"
    />
    <!-- Drawer used to display Billing Cycle details -->
    <app-form-dialog
      v-model="viewOpen"
      title="View Billing Cycle"
      size="sm"
      :saving="viewLoading"
      hide-save
      @cancel="closeView"
    >
      <div class="row q-col-gutter-lg">
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Name
          </div>
          <div class="text-2e fs-14">
            {{ viewBillingCycle.name || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge
            :class="
              viewBillingCycle.active
                ? 'active-badge'
                : 'inactive-badge'
            "
          >
            {{ viewBillingCycle.active ? "Active" : "Inactive" }}
          </q-badge>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Tenant
          </div>
          <div class="text-2e fs-14">
            {{ viewBillingCycle.tenantName || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewBillingCycle.createdBy || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{ formatDateTime(viewBillingCycle.createdOnUtc) }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewBillingCycle.updatedBy || "—" }}
          </div>
        </div>

        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{ viewBillingCycle.updatedBy ? formatDateTime(viewBillingCycle.updatedOnUtc): "—" }}
          </div>
        </div>
      </div>
    </app-form-dialog>
  </q-page>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import { date, debounce } from "quasar";
import { billingCycleApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { usePermissions } from "composables/usePermissions";
import { useListTable } from "composables/useListTable";
import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";
import AppFormDialog from "components/common/AppFormDialog.vue";
import CreateEdit from "modules/billing-cycle/components/CreateEdit.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
const { showDeleted, canManageDeleted } = useDeletedRecords();
// Check permissions for Billing Cycle actions.
const canRead = computed(() => has("billingCycles.read"));
const canWrite = computed(() => has("billingCycles.write"));
const canDelete = computed(() => has("billingCycles.delete"));
// Format UTC date values for display.
const formatDateTime = (value) => {
  if (!value) {
    return "—";
  }
  // Add UTC indicator when the API date does not include timezone information.
  const iso = /(Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : `${value}Z`;
  return date.formatDate(new Date(iso), "MM/DD/YYYY hh:mm A");
};

// Define the columns displayed in the Billing Cycle table.
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
    default: true,
    filterable: false
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
const { rows, loading, totalRecords, search, pagination, load, onRequest } = useListTable({
  pageKey: "billing-cycles",
  // Fetch Billing Cycles from the API with optional search and sorting.
  fetcher: ({ page, limit, sortBy, descending }) =>
    billingCycleApi.list({ page, limit, search: search.value || undefined, sortBy, descending, name: filters.name || undefined, active: filters.active ?? undefined, showDeleted: showDeleted.value })
      .then((response) => ({ data: response?.data || [], total: response?.meta?.totalRecords || 0 })),
  // Handle errors that occur during the API call.
  onError: (error) => { notify.error(getApiErrorMessage(error, "Unable to load billing cycles.")); }
});
const {
  filters,
  filterableColumns,
  filterChips,
  removeFilter,
  clearFilters
} = useColumnFilters(columns, rows, {
  server: true
});
// Reload the table when the search value changes.
// Debounce prevents an API call for every keystroke.
const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch([search, showDeleted, filters], reload, { deep: true });
const formOpen = ref(false);
const editing = ref(false);
const selectedBillingCycle = ref(null);
const viewOpen = ref(false);
const viewLoading = ref(false);
// Define the structure of the Billing Cycle being viewed.
const viewBillingCycle = ref({
  billingCycleId: null,
  name: "",
  active: true,
  tenantName: "",
  createdBy: null,
  createdOnUtc: null,
  updatedBy: null,
  updatedOnUtc: null,
  deleted: false
});
// Reset the viewBillingCycle to its initial state.
const resetViewBillingCycle = () => {
  viewBillingCycle.value = {
    billingCycleId: null,
    name: "",
    active: true,
    tenantName: "",
    createdBy: null,
    createdOnUtc: null,
    updatedBy: null,
    updatedOnUtc: null,
    deleted: false
  };
};
// Open the View drawer for the selected Billing Cycle.
const openView = async (row) => {
  resetViewBillingCycle();
  viewOpen.value = true;
  viewLoading.value = true;
  // Fetch the latest Billing Cycle details from the API to ensure accurate information is displayed.
  try {
    const billingCycle = await billingCycleApi.get(
      row.billingCycleId
    );
    viewBillingCycle.value = {
      billingCycleId: billingCycle?.billingCycleId,
      name: billingCycle?.name || "",
      active: billingCycle?.active ?? true,
      tenantName: billingCycle?.tenantName || "",
      createdBy: billingCycle?.createdBy || "",
      createdOnUtc: billingCycle?.createdOnUtc || null,
      updatedBy: billingCycle?.updatedBy || "",
      updatedOnUtc: billingCycle?.updatedOnUtc || null,
      deleted: billingCycle?.deleted ?? false
    };
  } catch (error) {
    viewOpen.value = false;
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load billing cycle."
      )
    );
  } finally {
    viewLoading.value = false;
  }
};
// Close the View drawer and reset the viewBillingCycle state.
const closeView = () => {
  viewOpen.value = false;
  resetViewBillingCycle();
};

// Open the form for creating a new Billing Cycle.
const openCreate = () => {
  selectedBillingCycle.value = null;
  editing.value = false;
  formOpen.value = true;
};

// Load the selected Billing Cycle and open the edit form.
const openEdit = async (row) => {
  try {
  // Get the latest Billing Cycle details from the API.
    const billingCycle = await billingCycleApi.get(
      row.billingCycleId
    );
    selectedBillingCycle.value = billingCycle;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load billing cycle."
      )
    );
  }
};
// Watch for changes to the formOpen state and load the form when it opens.
const toggleActive = async (row, active) => {
  const previousValue = row.active;
  // Update UI immediately.
  row.active = active;
  try {
    await billingCycleApi.update(row.billingCycleId, { name: row.name, active });
    notify.success(active ? "Billing cycle activated." : "Billing cycle deactivated.");
  } catch (error) {
    // Restore previous value if update fails.
    row.active = previousValue;
    notify.error(
      getApiErrorMessage(error, "Unable to update Billing Cycle status.")
    );
  }
};
// Delete the selected Billing Cycle.
const deleteBillingCycle = async (row) => {
  const confirmed = await confirm({
    title: "Delete Billing Cycle",
    message: `Delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  // Stop if the user cancels the operation.
  if (!confirmed) {
    return;
  }
  try {
    await billingCycleApi.remove(
      row.billingCycleId
    );
    notify.success("Billing cycle deleted.");
    load();
  } catch (error) {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to delete billing cycle."
      )
    );
  }
};
// Load Billing Cycles when the page is opened.
load();
</script>
