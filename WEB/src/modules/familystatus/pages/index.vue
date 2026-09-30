<template>
  <q-page padding>
    <!-- Page Header Component -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Family Status' }
      ]"
      title="Family Status"
      description="Manage your all family statuses here."
      :search="search"
      show-search
      search-placeholder="Search family status"
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Family Status"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <!-- Filter Drawer Component -->
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

    <!-- Core Data Table Grid Component -->
    <app-data-table
      page-key="family-statuses"
      :row-key="(row) => row.familyStatusId || row.FamilyStatusId || row.id || row.Id"
      title="Family Status"
      :rows="filteredRows"
      :columns="columns"
      :loading="loading"
      :total-records="filteredRows.length"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Family Status Name Column with Deleted Indicator -->
      <template #body-cell-familyStatusName="cell">
        <q-td :props="cell" :class="{ 'text-strike text-grey': cell.row.deleted || cell.row.Deleted }">
          {{ cell.row.familyStatusName || cell.row.FamilyStatusName || cell.row.name || cell.row.Name }}
          <q-badge v-if="cell.row.deleted || cell.row.Deleted" color="negative" class="q-ml-sm" dense>
            Deleted
          </q-badge>
        </q-td>
      </template>

      <!-- Interactive Status Toggle Column Slot -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <div class="row items-center q-gutter-x-sm">
            <q-toggle
              :model-value="cell.row.active ?? cell.row.Active ?? cell.row.isActive ?? cell.row.IsActive ?? true"
              @update:model-value="(val) => updateStatus(cell.row, val)"
              dense
              color="positive"
              :disable="cell.row.deleted || cell.row.Deleted"
            />
            <span :class="(cell.row.active ?? cell.row.Active ?? cell.row.isActive ?? cell.row.IsActive ?? true) ? 'text-positive' : 'text-grey'">
              {{ (cell.row.active ?? cell.row.Active ?? cell.row.isActive ?? cell.row.IsActive ?? true) ? "Active" : "Inactive" }}
            </span>
          </div>
        </q-td>
      </template>

      <!-- Row Actions Slot -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn
            flat
            round
            dense
            color="primary"
            icon="o_visibility"
            @click="openView(cell.row)"
          >
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <q-btn
            flat
            round
            dense
            color="primary"
            icon="o_edit"
            @click="openEdit(cell.row)"
            :disabled="cell.row.deleted || cell.row.Deleted"
          >
            <q-tooltip>Edit</q-tooltip>
          </q-btn>

          <q-btn
            flat
            round
            dense
            color="negative"
            icon="o_delete"
            @click="removeFamilyStatus(cell.row)"
            v-if="!(cell.row.deleted || cell.row.Deleted)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit Drawer Component -->
    <family-status-form
      v-model="formOpen"
      :editing-id="editingId"
      :initial-data="selectedRow"
      @saved="handleSaved"
    />

    <!-- View Details Drawer Component -->
    <family-status-view
      v-model="viewOpen"
      :record-id="viewRecordId"
    />
  </q-page>
</template>

<script setup>
import { ref, watch, onMounted } from "vue";
import { debounce } from "quasar";

import { familyStatusApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";

import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";

import FamilyStatusForm from "src/modules/familystatus/components/create_edit_status.vue";
import FamilyStatusView from "src/modules/familystatus/components/viewstatus.vue";

const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();

const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
};

const columns = [
  {
    name: "familyStatusName",
    label: "Family Status Name",
    field: (r) => r.familyStatusName || r.FamilyStatusName || r.name || r.Name,
    align: "left",
    sortable: true,
    default: true,
    filterable: true
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
    name: "createdBy",
    label: "Created By",
    field: (r) => r.createdBy || r.CreatedBy || r.created_by || "—",
    align: "left",
    sortable: true,
    default: true,
    filterable: false
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
    field: (r) => r.active ?? r.Active ?? r.isActive ?? r.IsActive ?? true,
    align: "left",
    sortable: true,
    default: true,
    filterOptions: [
      { label: "", value: true },
      { label: "", value: false }
    ]
  },
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "left",
    filterable: false
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
  pageKey: "family-statuses",
  defaultSortBy: "createdOnUtc",
  defaultDescending: true,
  fetcher: ({ sortBy, descending }) =>
    familyStatusApi.list({
      search: search.value || undefined,
      showDeleted: showDeleted.value,
      includeDeleted: showDeleted.value,
      sortBy: sortBy || 'createdOnUtc',
      descending: descending ?? true,
      includeInactive: true
    }).then((response) => {
      let items = response?.data?.items || response?.items || response?.data || [];

      
      items.sort((a, b) => {
        const valA = new Date(a.createdOnUtc || a.CreatedOnUtc || a.created_on_utc || a.createdOn || 0).getTime();
        const valB = new Date(b.createdOnUtc || b.CreatedOnUtc || b.created_on_utc || b.createdOn || 0).getTime();
        
        if (valA !== valB) {
          return valB - valA; 
        }
        
     
        const idA = a.familyStatusId || a.FamilyStatusId || a.id || a.Id || 0;
        const idB = b.familyStatusId || b.FamilyStatusId || b.id || b.Id || 0;
        return idB - idA;
      });

      return {
        data: items,
        total: response?.data?.totalCount || items.length
      };
    }),
  onError: (err) => notify.error(getApiErrorMessage(err))
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
watch(showDeleted, () => {
  pagination.value.page = 1;
  load();
});

onMounted(() => {
  pagination.value.sortBy = 'createdOnUtc';
  pagination.value.descending = true;
  load();
});

const formOpen = ref(false);
const editingId = ref(null);
const selectedRow = ref(null);

const openCreate = () => {
  editingId.value = null;
  selectedRow.value = null;
  formOpen.value = true;
};

const openEdit = (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }
  editingId.value = id;
  selectedRow.value = row;
  formOpen.value = true;
};

const updateStatus = async (row, newStatus) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id || row.Id;
  if (!id) return;

  const originalStatus = row.active ?? row.Active ?? row.isActive ?? row.IsActive ?? true;
  
  if (row.active !== undefined) row.active = newStatus;
  if (row.Active !== undefined) row.Active = newStatus;
  if (row.isActive !== undefined) row.isActive = newStatus;
  if (row.IsActive !== undefined) row.IsActive = newStatus;

  try {
    const payload = {
      familyStatusName: row.familyStatusName || row.FamilyStatusName || row.name || row.Name,
      active: newStatus,
      isActive: newStatus
    };
    await familyStatusApi.update(id, payload);
    notify.success("Status updated successfully.");
    await load();
  } catch (err) {
    if (row.active !== undefined) row.active = originalStatus;
    if (row.Active !== undefined) row.Active = originalStatus;
    if (row.isActive !== undefined) row.isActive = originalStatus;
    if (row.IsActive !== undefined) row.IsActive = originalStatus;
    notify.error(getApiErrorMessage(err));
  }
};

const handleSaved = async () => {
  pagination.value.page = 1;
  pagination.value.sortBy = 'createdOnUtc';
  pagination.value.descending = true;
  await load();
};

const viewOpen = ref(false);
const viewRecordId = ref(null);

const openView = (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }
  viewRecordId.value = id;
  viewOpen.value = true;
};

const removeFamilyStatus = async (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id || row.Id;
  if (!id) return;

  const familyStatusLabel = row.familyStatusName || row.FamilyStatusName || row.name || row.Name || 'this family status';
  const ok = await confirm({
    title: "Delete family status",
    message: `Are you sure you want to delete the family status "${familyStatusLabel}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) return;

  try {
    await familyStatusApi.delete(id);
    notify.success("Family status deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>