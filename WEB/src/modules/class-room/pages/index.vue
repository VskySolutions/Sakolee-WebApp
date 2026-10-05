<template>
  <q-page padding>
    <!-- Page Header Component -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Class Rooms' }
      ]"
      title="Class Rooms"
      description="Manage your all class rooms here."
      :search="search"
      show-search
      search-placeholder="Search class room"
      
      show-add
      add-label="Create Class Room"
      show-back
      @update:search="search = $event"
      
      @add="openCreate"
      @back="$router.back()"
    />
    <!-- Hidden/Commented Filter Drawer Attributes --> 
     <!-- show-filters
      :filter-count="filterChips.length"
      @filters="filterOpen = true" -->

    <!-- Hidden/Commented Filter Drawer Component -->
     
    <!-- Filter Drawer Component -->
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

    <!-- Core Data Table Grid Component -->
    <app-data-table
      page-key="class-rooms"
      :row-key="(row) => row.id || row.Id || row.classRoomId || row.ClassRoomId"
      title="Class Rooms"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="rows.length"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
    <!-- Hidden/Commented Filter Attributes --> 
      <!-- :rows="filteredRows"
      :total-records="filteredRows.length" -->

      <!-- Class Room Name Column with Deleted Indicator -->
      <template #body-cell-name="cell">
        <q-td :props="cell" :class="{ 'text-strike text-grey': cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted }">
          {{ cell.row.name || cell.row.Name || cell.row.classRoomName || cell.row.ClassRoomName }}
          <q-badge v-if="cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted" color="negative" class="q-ml-sm" dense>
            Deleted
          </q-badge>
        </q-td>
      </template>

      <!-- Location Name Column Slot -->
      <template #body-cell-location="cell">
        <q-td :props="cell">
          {{ resolveLocationName(cell.row) }}
        </q-td>
      </template>

      <!-- Interactive Status Toggle Column Slot -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <!-- <div class="row items-center q-gutter-x-sm"></div> -->
            <div class="flex flex-center">
             
            <q-toggle
              :model-value="cell.row.active ?? cell.row.Active ?? cell.row.isActive ?? cell.row.IsActive ?? true"
              @update:model-value="(val) => updateStatus(cell.row, val)"
              dense
              color="positive"
              :disable="cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted"
            />
            <!-- <span :class="(cell.row.active ?? cell.row.Active ?? cell.row.isActive ?? cell.row.IsActive ?? true) ? 'text-positive' : 'text-grey'">
              {{ (cell.row.active ?? cell.row.Active ?? cell.row.isActive ?? cell.row.IsActive ?? true) ? "Active" : "Inactive" }}
            </span> -->
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
            :disabled="cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted"
          >
            <q-tooltip>Edit</q-tooltip>
          </q-btn>

          <q-btn
            flat
            round
            dense
            color="negative"
            icon="o_delete"
            @click="removeClassRoom(cell.row)"
            v-if="!(cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit Form Drawer Component -->
    <CreateEditClassRoom
      v-model="formOpen"
      :editing-id="editingId"
      :initial-data="selectedRow"
      :location-options="locationOptions"
      @saved="handleSaved"
    />

    <!-- View Class Room Details Drawer Component -->
    <ViewClassRoom
      v-model="viewOpen"
      :view-id="selectedViewId"
      :location-map="locationMap"
    />
  </q-page>
</template>

<script setup>
import { ref, watch, onMounted } from "vue";
import { debounce } from "quasar";

import { classRoomApi, locationApi, getApiErrorMessage } from "services/api";

import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";

import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";

import CreateEditClassRoom from "src/modules/class-room/components/create_edit.vue";
import ViewClassRoom from "src/modules/class-room/components/view.vue";

const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();

const locationOptions = ref([]);
const locationMap = ref({});

const formOpen = ref(false);
const editingId = ref(null);
const selectedRow = ref(null);

const viewOpen = ref(false);
const selectedViewId = ref(null);

const loadLocations = async () => {
  try {
    const response = await locationApi.list({ limit: 1000 });
    const items = response?.data?.items || response?.items || response?.data || [];
    
    locationOptions.value = items.map(loc => ({
      id: loc.id || loc.LocationId || loc.locationId,
      name: loc.name || loc.LocationName || loc.locationName
    }));

    const map = {};
    locationOptions.value.forEach(loc => {
      if (loc.id) {
        map[loc.id] = loc.name;
      }
    });
    locationMap.value = map;
  } catch (err) {
    notify.error("Failed to load locations.");
  }
};

const resolveLocationName = (row) => {
  if (!row) return "—";
  
  const directName = row.locationName || row.LocationName || row.locationTitle || row.LocationTitle;
  if (directName) return directName;

  const nestedLoc = row.location || row.Location;
  if (nestedLoc) {
    if (typeof nestedLoc === 'string') return nestedLoc;
    const nestedName = nestedLoc.name || nestedLoc.Name || nestedLoc.locationName || nestedLoc.LocationName;
    if (nestedName) return nestedName;
  }

  const locId = row.locationId || row.LocationId;
  if (locId && locationMap.value[locId]) {
    return locationMap.value[locId];
  }
  return "—";
};

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
    label: "Class Room Name", 
    field: (r) => r.name || r.Name || r.classRoomName || r.ClassRoomName, 
    align: "left", 
    sortable: true, 
    default: true
  },
  { 
    name: "location", 
    label: "Location", 
    field: (r) => resolveLocationName(r), 
    align: "left", 
    sortable: true, 
    default: true 
  },
 
  { 
    name: "createdOnUtc", 
    label: "Created On", 
    field: (r) => r.createdOnUtc || r.CreatedOnUtc || r.created_on_utc || r.createdOn || null, 
    align: "left", 
    filterable: false, 
    sortable: true, 
    default: true, 
    format: (val) => formatDate(val) 
  },
  { 
    name: "createdBy", 
    label: "Created By", 
    field: (r) => r.createdBy || r.CreatedBy || "—", 
    align: "left", 
    sortable: true, 
    default: true, 
    filterable: false 
  },
  { 
    name: "updatedOnUtc", 
    label: "Updated On", 
    field: (r) => r.updatedOnUtc || r.UpdatedOnUtc || r.updated_on_utc || r.updatedOn || null, 
    align: "left", 
    sortable: true, 
    default: false, 
    filterable: false, 
    format: (val) => formatDate(val)
  },
  {
    name: "updatedBy",
    label: "Updated By",
    field: (r) => r.updatedBy || r.UpdatedBy || "—",
    align: "left",
    sortable: true,
    default: false,
    filterable: false
  },
   { 
    name: "active", 
    label: "Status", 
    field: (r) => r.active ?? r.Active ?? r.isActive ?? r.IsActive ?? true, 
    align: "center", 
    filterable: false, 
    sortable: true, 
    default: true 
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
  pageKey: "class-rooms",
  defaultSortBy: "createdOnUtc",
  defaultDescending: true,

  fetcher: (params) => {
    const sortBy = params?.sortBy || pagination.value.sortBy || "createdOnUtc";
    const descending = params?.descending ?? pagination.value.descending ?? true;

    return classRoomApi.list({
      search: search.value || undefined,
      showDeleted: showDeleted.value,
      includeDeleted: showDeleted.value,
      sortBy,
      descending,
      includeInactive: true
    }).then((response) => {
      let items = response?.data?.items || response?.items || response?.data || [];

      items.sort((a, b) => {
        const fieldA = pagination.value.sortBy || sortBy;
        const isDesc = pagination.value.descending ?? descending;

        if (fieldA === 'createdOnUtc' || fieldA === 'updatedOnUtc') {
          const valA = new Date(a.createdOnUtc || a.CreatedOnUtc || 0).getTime();
          const valB = new Date(b.createdOnUtc || b.CreatedOnUtc || 0).getTime();
          if (valA !== valB) {
            return isDesc ? valB - valA : valA - valB;
          }
        }

        const idA = a.id || a.Id || a.classRoomId || 0;
        const idB = b.id || b.Id || b.classRoomId || 0;
        if (typeof idA === 'number' && typeof idB === 'number' && idA !== idB) {
          return isDesc ? idB - idA : idA - idB;
        }

        const nameA = String(a.name || a.Name || '').toLowerCase();
        const nameB = String(b.name || b.Name || '').toLowerCase();
        return isDesc ? nameB.localeCompare(nameA) : nameA.localeCompare(nameB);
      });

      return {
        data: items,
        total: response?.data?.totalCount || items.length
      };
    });
  },

  onError: (err) => notify.error(getApiErrorMessage(err))
});

// Commented filters are hidden in Master Module
// const filterOpen = ref(false);

// const {
//   filters,
//   filterableColumns,
//   filteredRows,
//   filterChips,
//   removeFilter,
//   clearFilters
// } = useColumnFilters(columns, rows, {
//   server: false
// });

const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch(search, reload);
watch(showDeleted, () => {
  pagination.value.page = 1;
  load();
});

onMounted(async () => {
  pagination.value.sortBy = 'createdOnUtc';
  pagination.value.descending = true;
  await loadLocations();
  load();
});

const openCreate = () => {
  editingId.value = null;
  selectedRow.value = null;
  formOpen.value = true;
};

const openEdit = (row) => {
  const id = row.id || row.Id || row.classRoomId || row.ClassRoomId;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }
  editingId.value = id;
  selectedRow.value = row;
  formOpen.value = true;
};

const updateStatus = async (row, newStatus) => {
  const id = row.id || row.Id || row.classRoomId || row.ClassRoomId;
  if (!id) return;

  const originalStatus = row.active ?? row.Active ?? row.isActive ?? row.IsActive ?? true;
  
  if (row.active !== undefined) row.active = newStatus;
  if (row.Active !== undefined) row.Active = newStatus;
  if (row.isActive !== undefined) row.isActive = newStatus;
  if (row.IsActive !== undefined) row.IsActive = newStatus;

  try {
    const payload = {
      name: row.name || row.Name || row.classRoomName || row.ClassRoomName,
      locationId: row.locationId || row.LocationId,
      active: newStatus,
      isActive: newStatus
    };
    await classRoomApi.update(id, payload);
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

const handleSaved = () => {
  pagination.value.page = 1;
  pagination.value.sortBy = 'createdOnUtc';
  pagination.value.descending = true;
  load();
};

const openView = (row) => {
  const id = row.id || row.Id || row.classRoomId || row.ClassRoomId;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }
  selectedViewId.value = id;
  viewOpen.value = true;
};

const removeClassRoom = async (row) => {
  const id = row.id || row.Id || row.classRoomId || row.ClassRoomId;
  if (!id) return;

  const roomLabel = row.name || row.Name || 'this class room';
  const ok = await confirm({
    title: "Delete class room",
    message: `Are you sure you want to delete the class room "${roomLabel}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) return;

  try {
    await classRoomApi.delete(id);
    notify.success("Class room deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>