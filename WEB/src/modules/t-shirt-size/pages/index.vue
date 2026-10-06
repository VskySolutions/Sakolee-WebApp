<template>
  <q-page padding>
    <!-- Page header with breadcrumbs, search, filters, and create button -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'T-Shirt Sizes' }
      ]"
      title="T-Shirt Sizes"
      description="Manage your T-Shirt sizes here."
      :search="search"
      show-search
      search-placeholder="Search T-Shirt size name"
     
      :show-add="canWrite"
      add-label="Create T-Shirt Size"
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
    <app-filter-drawer
      v-model="filterOpen"
      :chips="filterChips"
      @remove="removeFilter"
      @clear="clearFilters"
    >
      <app-column-filters
        v-model="filters"
        :columns="filterableColumns"
      />
      <!-- Show deleted records -->
      <q-toggle
        v-if="canManageDeleted"
        v-model="showDeleted"
        label="Show deleted?"
        dense
        class="q-mt-md"
      />
    </app-filter-drawer>
    <!-- T-Shirt Size table -->
    <app-data-table
      page-key="t-shirt-sizes"
      row-key="tShirtSizeId"
      title="T-Shirt Sizes"
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
      <!-- Action buttons -->
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
            @click="deleteTShirtSize(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <!-- Create/Edit dialog -->
    <create-edit
      v-model="formOpen"
      :editing="editing"
      :t-shirt-size="selectedTShirtSize"
      @saved="load"
    />
    <!-- View T-Shirt Size -->
    <app-form-dialog
      v-model="viewOpen"
      title="View T-Shirt Size"
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
            {{ viewTShirtSize.name || "—" }}
          </div>
        </div>
        <!-- Status -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge
            :class="
              viewTShirtSize.active
                ? 'active-badge'
                : 'inactive-badge'
            "
          >
            {{
              viewTShirtSize.active
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
            {{ viewTShirtSize.tenantName || "—" }}
          </div>
        </div>
        <!-- Created By -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewTShirtSize.createdBy || "—" }}
          </div>
        </div>
        <!-- Created On -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{ formatDateTime(viewTShirtSize.createdOnUtc) }}
          </div>
        </div>
        <!-- Updated By -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewTShirtSize.updatedBy || "—" }}
          </div>
        </div>
        <!-- Updated On -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{ viewTShirtSize.updatedBy ? formatDateTime(viewTShirtSize.updatedOnUtc): "—" }}
          </div>
        </div>
      </div>
    </app-form-dialog>
  </q-page>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import { date, debounce } from "quasar";
import {
  tShirtSizeApi,
  getApiErrorMessage
} from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { usePermissions } from "composables/usePermissions";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";
import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";
import CreateEdit from "modules/t-shirt-size/components/CreateEdit.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
const {
  showDeleted,
  canManageDeleted
} = useDeletedRecords();
/* ---------------------------------
 * Permissions
 * --------------------------------- */
const canRead = computed(() =>
  has("tShirtSizes.read")
);
const canWrite = computed(() =>
  has("tShirtSizes.write")
);
const canDelete = computed(() =>
  has("tShirtSizes.delete")
);
/* ---------------------------------
 * Date formatting
 * --------------------------------- */
const formatDateTime = (value) => {
  if (!value) {
    return "—";
  }
  // Add UTC indicator when the API date
  // does not include timezone information.
  const iso =
    /(Z|[+-]\d{2}:\d{2})$/i.test(value)
      ? value
      : `${value}Z`;
  return date.formatDate(
    new Date(iso),
    "MM/DD/YYYY hh:mm A"
  );
};

/* ---------------------------------
 * Table columns
 * --------------------------------- */
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
/* ---------------------------------
 * Filter state
 * --------------------------------- */
const filterOpen = ref(false);
/* ---------------------------------
 * List table
 * --------------------------------- */
const { rows, loading, totalRecords, search, pagination, load, onRequest } = useListTable({
  pageKey: "t-shirt-sizes",
  // Fetch T-Shirt Sizes from the API.
  fetcher: ({ page, limit, sortBy, descending }) => tShirtSizeApi.list({ page, limit, search: search.value || undefined, sortBy, descending, name:filters.name || undefined, active: filters.active ?? undefined, showDeleted: showDeleted.value })
    .then((response) => ({ data: response?.data || [], total: response?.meta?.totalRecords || 0 })),
  // Handle API errors.
  onError: (error) => {
    notify.error(
      getApiErrorMessage(error,"Unable to load T-Shirt sizes.")
    );
  }
});
/* ---------------------------------
 * Server-side column filters
 * --------------------------------- */
const { filters, filterableColumns, filterChips, removeFilter, clearFilters } = useColumnFilters(columns, rows, { server: true });
/* ---------------------------------
 * Reload when filters/search change
 * --------------------------------- */
const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);
watch([search, showDeleted, filters], reload, { deep: true });
/* ---------------------------------
 * Create/Edit state
 * --------------------------------- */
const formOpen = ref(false);
const editing = ref(false);
const selectedTShirtSize = ref(null);
/* ---------------------------------
 * View state
 * --------------------------------- */
const viewOpen = ref(false);
const viewLoading = ref(false);
const viewTShirtSize = ref({
  tShirtSizeId: null,
  name: "",
  tenantName: "",
  createdBy: null,
  createdOnUtc: null,
  updatedBy: null,
  updatedOnUtc: null,
  active: true,
  deleted: false
});
/* ---------------------------------
 * Reset view state
 * --------------------------------- */
const resetViewTShirtSize = () => {
  viewTShirtSize.value = {
    tShirtSizeId: null,
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
/* ---------------------------------
 * Open View
 * --------------------------------- */
const openView = async (row) => {
  resetViewTShirtSize();
  viewOpen.value = true;
  viewLoading.value = true;
  try {
    const tShirtSize = await tShirtSizeApi.get(row.tShirtSizeId);
    viewTShirtSize.value = {
      tShirtSizeId: tShirtSize?.tShirtSizeId,
      name: tShirtSize?.name || "",
      tenantName: tShirtSize?.tenantName || "",
      createdBy: tShirtSize?.createdBy || "",
      createdOnUtc: tShirtSize?.createdOnUtc || null,
      updatedBy: tShirtSize?.updatedBy || "",
      updatedOnUtc: tShirtSize?.updatedOnUtc || null,
      active: tShirtSize?.active ?? true,
      deleted: tShirtSize?.deleted ?? false
    };
  } catch (error) {
    viewOpen.value = false;
    notify.error(
      getApiErrorMessage(error, "Unable to load T-Shirt size.")
    );
  } finally {
    viewLoading.value = false;
  }
};
/* ---------------------------------
 * Close View
 * --------------------------------- */
const closeView = () => {
  viewOpen.value = false;
  resetViewTShirtSize();
};
/* ---------------------------------
 * Create
 * --------------------------------- */
const openCreate = () => {
  selectedTShirtSize.value = null;
  editing.value = false;
  formOpen.value = true;
};
/* ---------------------------------
 * Edit
 * --------------------------------- */
const openEdit = async (row) => {
  try {
    const tShirtSize = await tShirtSizeApi.get(row.tShirtSizeId);
    selectedTShirtSize.value = tShirtSize;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    notify.error(
      getApiErrorMessage(error, "Unable to load T-Shirt size.")
    );
  }
};
/* ---------------------------------
 * Toggle Active
 * --------------------------------- */
const toggleActive = async (row, active) => { const previousValue = row.active;
  // Update UI immediately.
  row.active = active;
  try {
    await tShirtSizeApi.update(row.tShirtSizeId, { name: row.name, active });
    notify.success(active ? "T-Shirt size activated." : "T-Shirt size deactivated.");
  } catch (error) {
    // Restore previous value if update fails.
    row.active = previousValue;
    notify.error(getApiErrorMessage(error, "Unable to update T-Shirt size status.")
    );
  }
};
/* ---------------------------------
 * Delete
 * --------------------------------- */
const deleteTShirtSize = async (
  row
) => {
  const confirmed = await confirm({
    title: "Delete T-Shirt Size",
    message: `Delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  // Stop if the user cancels.
  if (!confirmed) {
    return;
  }
  try {
    await tShirtSizeApi.remove(row.tShirtSizeId);
    notify.success("T-Shirt size deleted.");
    load();
  } catch (error) {
    notify.error(getApiErrorMessage(error, "Unable to delete T-Shirt size.")
    );
  }
};
/* ---------------------------------
 * Initial load
 * --------------------------------- */
load();
</script>
