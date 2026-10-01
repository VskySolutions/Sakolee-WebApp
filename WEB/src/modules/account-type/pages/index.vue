<template>
  <q-page padding>
    <!-- Page header -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' },{ label: 'Account Type' }]"
      title="Account Type"
      description="Manage your Account Type here."
      :search="search"
      show-search
      search-placeholder="Search Account Type name"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canWrite"
      add-label="Create Account Type"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />
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
      <q-toggle
        v-if="canManageDeleted"
        v-model="showDeleted"
        label="Show deleted?"
        dense
        class="q-mt-md"
      />
    </app-filter-drawer>
    <!-- Account Type table -->
    <app-data-table
      page-key="account-type"
      row-key="accountTypeId"
      title="Account Type"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Active status -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-toggle
            :model-value="cell.row.active"
            :disable="!canWrite || cell.row.deleted"
            color="positive"
            @update:model-value="
              toggleActive(cell.row, $event)
            "
          >
            <q-tooltip>
              {{
                cell.row.deleted
                  ? "Deleted record"
                  : cell.row.active
                    ? "Active"
                    : "Inactive"
              }}
            </q-tooltip>
          </q-toggle>
        </q-td>
      </template>
      <!-- Actions -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <!-- View -->
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
          <!-- Edit -->
          <q-btn
            v-if="canWrite"
            type="a"
            flat
            round
            dense
            color="primary"
            icon="o_edit"
            :disable="cell.row.deleted"
            @click="openEdit(cell.row)"
          >
            <q-tooltip>
              {{
                cell.row.deleted
                  ? "Deleted record"
                  : "Edit"
              }}
            </q-tooltip>
          </q-btn>
          <!-- Delete -->
          <q-btn
            v-if="canDelete"
            type="a"
            flat
            round
            dense
            color="negative"
            icon="o_delete"
            :disable="cell.row.deleted"
            @click="deleteAccountType(cell.row)"
          >
            <q-tooltip>
              {{
                cell.row.deleted
                  ? "Deleted record"
                  : "Delete"
              }}
            </q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <!-- Create / Edit dialog -->
    <create-edit
      v-model="formOpen"
      :editing="editing"
      :account-type="selectedAccountType"
      @saved="load"
    />
    <!-- View dialog -->
    <app-form-dialog
      v-model="viewOpen"
      title="View Account Type"
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
            {{ viewAccountType.name || "—" }}
          </div>
        </div>
        <!-- Status -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge :class="viewAccountType.active ? 'active-badge': 'inactive-badge'">
            {{ viewAccountType.active ? "Active" : "Inactive" }}
          </q-badge>
        </div>
        <!-- Tenant -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Tenant
          </div>
          <div class="text-2e fs-14">
            {{ viewAccountType.tenantName || "—" }}
          </div>
        </div>
        <!-- Created By -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewAccountType.createdBy || "—" }}
          </div>
        </div>
        <!-- Created On -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{ formatDateTime( viewAccountType.createdOnUtc ) }}
          </div>
        </div>
        <!-- Updated By -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewAccountType.updatedBy || "—" }}
          </div>
        </div>
        <!-- Updated On -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{ viewAccountType.updatedBy ? formatDateTime( viewAccountType.updatedOnUtc ) : "—" }}
          </div>
        </div>
      </div>
    </app-form-dialog>
  </q-page>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import { date, debounce } from "quasar";
import { accountTypeApi, getApiErrorMessage } from "services/api";
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
import CreateEdit from "modules/account-type/components/CreateEdit.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
const { showDeleted, canManageDeleted } = useDeletedRecords();
/*
 * Permissions.
 */
const canRead = computed(() =>
  has("accountTypes.read")
);
const canWrite = computed(() =>
  has("accountTypes.write")
);
const canDelete = computed(() =>
  has("accountTypes.delete")
);
/*
 * Format UTC date.
 */
const formatDateTime = (value) => {
  if (!value) {
    return "—";
  }
  const iso = /(Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : `${value}Z`;
  return date.formatDate(new Date(iso), "MM/DD/YYYY hh:mm A");
};
/*
 * Table columns.
 */
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
/*
 * List table.
 */
const { rows, loading, totalRecords, search, pagination, load, onRequest } = useListTable({
  pageKey: "account-type",
  fetcher: ({ page, limit, sortBy, descending }) => accountTypeApi.list({ page, limit, search: search.value || undefined, sortBy, descending, name: filters.name || undefined, active: filters.active ?? undefined, showDeleted: showDeleted.value })
    .then((response) => ({ data: response?.data || [], total: response?.meta?.totalRecords || 0 }
    )),
  onError: (error) => {
    notify.error(getApiErrorMessage(error, "Unable to load Account Type records."));
  }
});

/*
 * Server-side filters.
 */
const {
  filters,
  filterableColumns,
  filterChips,
  removeFilter,
  clearFilters
} = useColumnFilters(
  columns,
  rows,
  {
    server: true
  }
);

/*
 * Reload after filters/search change.
 */
const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch([search, showDeleted, filters], reload, { deep: true });
/*
 * Create / Edit dialog.
 */
const formOpen = ref(false);
const editing = ref(false);
const selectedAccountType = ref(null);
/*
 * View dialog.
 */
const viewOpen = ref(false);
const viewLoading = ref(false);
const viewAccountType = ref({
  accountTypeId: null,
  name: "",
  tenantName: "",
  createdBy: null,
  createdOnUtc: null,
  updatedBy: null,
  updatedOnUtc: null,
  active: true,
  deleted: false
});
/*
 * Reset view object.
 */
const resetViewAccountType = () => {
  viewAccountType.value = {
    accountTypeId: null,
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
/*
 * Open View.
 */
const openView = async (row) => {
  resetViewAccountType();
  viewOpen.value = true;
  viewLoading.value = true;
  try {
    const accountType = await accountTypeApi.get(row.accountTypeId);
    viewAccountType.value = {
      accountTypeId: accountType?.accountTypeId,
      name: accountType?.name || "",
      tenantName: accountType?.tenantName || "",
      createdBy: accountType?.createdBy || "",
      createdOnUtc: accountType?.createdOnUtc || null,
      updatedBy: accountType?.updatedBy || "",
      updatedOnUtc: accountType?.updatedOnUtc || null,
      active: accountType?.active ?? true,
      deleted: accountType?.deleted ?? false
    };
  } catch (error) {
    viewOpen.value = false;
    notify.error(getApiErrorMessage(error, "Unable to load Account Type record."));
  } finally {
    viewLoading.value = false;
  }
};
/*
 * Close View.
 */
const closeView = () => {
  viewOpen.value = false;
  resetViewAccountType();
};
/*
 * Open Create.
 */
const openCreate = () => {
  selectedAccountType.value = null;
  editing.value = false;
  formOpen.value = true;
};
/*
 * Open Edit.
 */
const openEdit = async (row) => {
  try {
    const accountType = await accountTypeApi.get(row.accountTypeId);
    selectedAccountType.value = accountType;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    notify.error(getApiErrorMessage(error, "Unable to load Account Type record."));
  }
};

/*
 * Toggle Active / Inactive.
 */
const toggleActive = async (row, active) => {
  if (row.deleted) {
    return;
  }
  const previousValue = row.active;
  row.active = active;
  try {
    await accountTypeApi.update(row.accountTypeId, { name: row.name, active });
    notify.success(active ? "Account Type activated." : "Account Type deactivated.");
  } catch (error) {
    row.active = previousValue;
    notify.error(getApiErrorMessage(error, "Unable to update Account Type status."));
  }
};

/*
 * Delete Account Type.
 */
const deleteAccountType = async (row) => {
  if (row.deleted) {
    return;
  }
  const confirmed = await confirm({
    title: "Delete Account Type",
    message: `Delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!confirmed) {
    return;
  }
  try {
    await accountTypeApi.remove(row.accountTypeId);
    notify.success("Account Type deleted.");
    load();
  } catch (error) {
    notify.error(getApiErrorMessage(error, "Unable to delete Account Type record."));
  }
};
/*
 * Initial load.
 */
load();
</script>
