<template>
  <q-page padding>
    <!-- Page Header Component -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'Membership Types' }]"
      title="Membership Types"
      description="Manage your all membership types here."
      :search="search"
      show-search
      search-placeholder="Search membership types"
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Membership Type"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <!-- Filter Drawer Component -->
    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-column-filters v-model="filters" :columns="filterableColumns" />
      <q-toggle v-if="canManageDeleted" v-model="showDeleted" label="Show deleted?" dense class="q-mt-md" />
    </app-filter-drawer>

    <!-- Core Data Table Grid Component -->
    <app-data-table
      page-key="membership-types"
      :row-key="(row) => row.membershipTypeId || row.MembershipTypeId || row.id || row.Id"
      title="Membership Types"
      :rows="filteredRows"
      :columns="columns"
      :loading="loading"
      :total-records="filteredRows.length"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Name Cell with Deleted Indicator -->
      <template #body-cell-name="cell">
        <q-td :props="cell" :class="{ 'text-strike text-grey': cell.row.deleted || cell.row.Deleted }">
          {{ cell.row.name || cell.row.Name || cell.row.membershipTypeName || cell.row.MembershipTypeName }}
          <q-badge v-if="cell.row.deleted || cell.row.Deleted" color="negative" class="q-ml-sm" dense>
            Deleted
          </q-badge>
        </q-td>
      </template>

      <!-- Interactive Status Toggle Column -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <div class="flex flex-center">
            <q-toggle
              :model-value="cell.row.active ?? cell.row.Active ?? cell.row.is_active ?? true"
              @update:model-value="(val) => updateStatus(cell.row, val)"
              dense
              color="positive"
              :disable="cell.row.deleted || cell.row.Deleted"
            />
          </div>
        </q-td>
      </template>

      <!-- Actions Column -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" @click="openView(cell.row)">
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <q-btn flat round dense color="primary" icon="o_edit" @click="openEdit(cell.row)" :disabled="cell.row.deleted || cell.row.Deleted">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn flat round dense color="negative" icon="o_delete" @click="removeMembershipType(cell.row)" v-if="!(cell.row.deleted || cell.row.Deleted)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit Form Component -->
    <membership-type-form
      v-model="formDrawerOpen"
      :editing-id="editingId"
      :initial-data="selectedRow"
      @saved="handleSaved"
    />

    <!-- View Component -->
    <membership-type-view
      v-model="viewDrawerOpen"
      :record-id="viewRecordId"
    />
  </q-page>
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { debounce } from "quasar";

import { membershipTypeApi, getApiErrorMessage } from "services/api";

import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";

import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";

import MembershipTypeForm from "src/modules/membership-type/components/create_edit.vue";
import MembershipTypeView from "src/modules/membership-type/components/view.vue";

const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();

// const formatDate = (value) => {
//   if (!value) return "—";
//   const date = new Date(value);
//   return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
// };

const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  // Check if date is invalid or the default .NET MinValue (0001-01-01)
  if (isNaN(date.getTime()) || date.getFullYear() <= 1) return "—";
  return date.toLocaleString();
};

const columns = [
  {
    name: "name",
    label: "Membership Type Name",
    field: (r) => r.name || r.Name || r.membershipTypeName || r.MembershipTypeName,
    align: "left",
    sortable: true,
    default: true
  },
  {
    name: "createdBy",
    label: "Created By",
    field: (r) => r.createdBy || r.CreatedBy || r.created_by || "—",
    align: "left",
    sortable: true,
    default: true,
    filterable: false
  },
  {
    name: "createdOnUtc",
    label: "Created On",
    field: (r) => r.createdOnUtc || r.CreatedOnUtc || r.created_on_utc || r.createdOn || r.CreatedOn || r.created_on || null,
    align: "left",
    sortable: true,
    default: true,
    filterable: false,
    format: (val) => formatDate(val)
  },
  {
    name: "updatedBy",
    label: "Updated By",
    field: (r) => r.updatedBy || r.UpdatedBy || r.updated_by || "—",
    align: "left",
    sortable: true,
    default: false,
    filterable: false
  },
  {
    name: "updatedOnUtc",
    label: "Updated On",
    field: (r) => r.updatedOnUtc || r.UpdatedOnUtc || r.updated_on_utc || r.updatedOn || r.UpdatedOn || r.updated_on || null,
    align: "left",
    sortable: true,
    default: false,
    filterable: false,
    format: (val) => formatDate(val)
  },
  {
    name: "active",
    label: "Status",
    field: (r) => r.active ?? r.Active ?? r.is_active ?? true,
    align: "center",
    sortable: true,
    default: true,
    filterOptions: [
      { label: "Active", value: true },
      { label: "Inactive", value: false }
    ]
  },
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "left"
  }
];

const {
  rows,
  loading,
  search,
  pagination,
  load,
  onRequest
} = useListTable({
  pageKey: "membership-types",
  fetcher: ({ sortBy, descending }) =>
    membershipTypeApi.list({
      search: search.value || undefined,
      showDeleted: showDeleted.value,
      sortBy: sortBy || 'createdOnUtc',
      descending: descending ?? true,
      includeInactive: true
    }).then((response) => {
      const items = response?.data?.items || response?.items || response?.data || [];
      return {
        data: items,
        total: items.length
      };
    }),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

watch(showDeleted, () => {
  pagination.value.page = 1;
  load();
});

const filterOpen = ref(false);

const {
  filters,
  filterableColumns,
  filteredRows,
  filterChips,
  removeFilter,
  clearFilters
} = useColumnFilters(columns, rows, {
  server: false
});

const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch(search, reload);

onMounted(() => {
  pagination.value.sortBy = 'createdOnUtc';
  pagination.value.descending = true;
  load();
});

const formDrawerOpen = ref(false);
const editingId = ref(null);
const selectedRow = ref(null);

const openCreate = () => {
  editingId.value = null;
  selectedRow.value = null;
  formDrawerOpen.value = true;
};

const openEdit = (row) => {
  const id = row.membershipTypeId || row.MembershipTypeId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }
  editingId.value = id;
  selectedRow.value = row;
  formDrawerOpen.value = true;
};

// Function to update status directly from table toggle and sync with DB
const updateStatus = async (row, newStatus) => {
  const id = row.membershipTypeId || row.MembershipTypeId || row.id || row.Id;
  if (!id) return;

  const originalStatus = row.active ?? row.Active ?? row.is_active ?? true;
  
  if (row.active !== undefined) row.active = newStatus;
  if (row.Active !== undefined) row.Active = newStatus;
  if (row.is_active !== undefined) row.is_active = newStatus;

  try {
    const payload = {
      name: row.name || row.Name || row.membershipTypeName || row.MembershipTypeName,
      active: newStatus
    };
    await membershipTypeApi.update(id, payload);
    notify.success("Status updated successfully.");
    await load();
  } catch (err) {
    // Revert on error
    if (row.active !== undefined) row.active = originalStatus;
    if (row.Active !== undefined) row.Active = originalStatus;
    if (row.is_active !== undefined) row.is_active = originalStatus;
    notify.error(getApiErrorMessage(err));
  }
};

const handleSaved = async () => {
  pagination.value.page = 1;
  pagination.value.sortBy = "createdOnUtc";
  pagination.value.descending = true;
  await load();
};

const viewDrawerOpen = ref(false);
const viewRecordId = ref(null);

const openView = (row) => {
  const id = row.membershipTypeId || row.MembershipTypeId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }
  viewRecordId.value = id;
  viewDrawerOpen.value = true;
};

const removeMembershipType = async (row) => {
  const id = row.membershipTypeId || row.MembershipTypeId || row.id || row.Id;
  if (!id) return;

  const methodLabel = row.name || row.Name || row.membershipTypeName || row.MembershipTypeName || 'this membership type';
  const ok = await confirm({
    title: "Delete membership type",
    message: `Are you sure you want to delete the membership type "${methodLabel}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) return;

  try {
    await membershipTypeApi.delete(id);
    notify.success("Membership type deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>