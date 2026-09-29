//Class Rooom index
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
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Class Room"
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
      page-key="class-rooms"
      :row-key="(row) => row.id || row.Id || row.classRoomId || row.ClassRoomId"
      title="Class Rooms"
      :rows="filteredRows"
      :columns="columns"
      :loading="loading"
      :total-records="filteredRows.length"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Location Name Column Slot -->
      <template #body-cell-location="cell">
        <q-td :props="cell">
          {{ resolveLocationName(cell.row) }}
        </q-td>
      </template>

      <!-- Status Column Slot -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">
            {{ cell.value ? "Active" : "Inactive" }}
          </q-badge>
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
// Import necessary Vue and Quasar utilities
import { ref, watch, onMounted } from "vue";
import { debounce } from "quasar";

// Import API services and composables
import { classRoomApi, locationApi, getApiErrorMessage } from "services/api";

// Import composables for notifications, confirmations, and table management
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

// Reactive state variables
const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();

const locationOptions = ref([]);
const locationMap = ref({});

const formOpen = ref(false);
const editingId = ref(null);

const viewOpen = ref(false);
const selectedViewId = ref(null);

/** * Fetch available locations for selection dropdown and mapping lookup.*/

const loadLocations = async () => {
  try {
    // Fetch all locations with a high limit to ensure we get all available locations
    const response = await locationApi.list({ limit: 1000 });
    const items = response?.data?.items || response?.items || response?.data || [];
    
    // Map the fetched locations to a simplified structure for dropdown options
    locationOptions.value = items.map(loc => ({
      id: loc.id || loc.LocationId || loc.locationId,
      name: loc.name || loc.LocationName || loc.locationName
    }));

    // Create a mapping of location IDs to names for quick lookup in the table
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

/**
 * Helper to dynamically resolve location name.
 */
const resolveLocationName = (row) => {
  if (!row) return "—";
  
  //  Check direct properties on the row object
  const directName = row.locationName || row.LocationName || row.locationTitle || row.LocationTitle;
  if (directName) return directName;

  // Check nested location object (e.g., row.location.name or row.Location.Name)
  const nestedLoc = row.location || row.Location;
  if (nestedLoc) {
    if (typeof nestedLoc === 'string') return nestedLoc;
    const nestedName = nestedLoc.name || nestedLoc.Name || nestedLoc.locationName || nestedLoc.LocationName;
    if (nestedName) return nestedName;
  }

  // Fallback to locationMap lookup using locationId
  const locId = row.locationId || row.LocationId;
  if (locId && locationMap.value[locId]) {
    return locationMap.value[locId];
  }
  return "—";
};

// Helper to format date values for display in the table
const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
};

// Define the columns for the data table, including custom field resolvers and formatting
const columns = [
  { name: "name",   label: "Class Room Name",   field: (r) => r.name || r.Name || r.classRoomName || r.ClassRoomName,   align: "left",  sortable: true, default: true},
  { name: "location",   label: "Location",  field: (r) => resolveLocationName(r),   align: "left",  sortable: true,  default: true  },
  { name: "active", label: "Status",    field: (r) => r.active ?? r.Active ?? true, align: "left",  filterable: false, sortable: true, default: true   },
  { name: "createdOnUtc", label: "Created On",  field: (r) => r.createdOnUtc || r.CreatedOnUtc || r.created_on_utc || r.createdOn || null,  align: "left",  filterable: false, sortable: true, default: true,   format: (val) => formatDate(val)    },
  { name: "createdBy",  label: "Created By",    field: (r) => r.createdBy || r.CreatedBy || "—",    align: "left",  sortable: true, default: true,  filterable: false   },
  { name: "updatedOnUtc",   label: "Updated On",    field: (r) => r.updatedOnUtc || r.UpdatedOnUtc || r.updated_on_utc || r.updatedOn || null,  align: "left",  sortable: true, default: true,  filterable: false,  format: (val) => formatDate(val)},
  {
    name: "updatedBy",
    label: "Updated By",
    field: (r) => r.updatedBy || r.UpdatedBy || "—",
    align: "left",
    sortable: true,
    default: true,
    filterable: false
  },
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "left"
  }
];

// Use the useListTable composable to manage table data, pagination, and sorting
const { rows, loading, search,  pagination, load,   onRequest} = 
useListTable({ pageKey: "class-rooms", defaultSortBy: "createdOnUtc",defaultDescending: true,

// Custom fetcher function to retrieve class room data from the API
  fetcher: (params) => {
    const sortBy = params?.sortBy || pagination.value.sortBy || "createdOnUtc";
    const descending = params?.descending ?? pagination.value.descending ?? true;

    // Call the classRoomApi.list method with search and sorting parameters
    return classRoomApi.list({
      search: search.value || undefined,
      sortBy,
      descending
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

// Reactive state for filter drawer visibility
const filterOpen = ref(false);

// Use the useColumnFilters composable to manage column-based filtering
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

// Debounced reload function to reset pagination and reload data when search changes
const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch(search, reload);

// onMounted lifecycle hook to load initial data and locations when the component is mounted
onMounted(async () => {
  pagination.value.sortBy = 'createdOnUtc';
  pagination.value.descending = true;
  await loadLocations();
  load();
});

// Functions to open the create, edit, and view drawers with appropriate data
const openCreate = () => {
  editingId.value = null;
  formOpen.value = true;
};

// Function to open the edit drawer with the selected row's ID
const openEdit = (row) => {
  const id = row.id || row.Id || row.classRoomId || row.ClassRoomId;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }
  editingId.value = row; // Passing full row or id depending on form handling
  formOpen.value = true;
};

// Function to open the view drawer with the selected row's ID
const openView = (row) => {
  const id = row.id || row.Id || row.classRoomId || row.ClassRoomId;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }
  selectedViewId.value = id;
  viewOpen.value = true;
};

// Function to handle the saved event from the create/edit form, resetting pagination and reloading data
const handleSaved = () => {
  pagination.value.page = 1;
  pagination.value.sortBy = 'createdOnUtc';
  pagination.value.descending = true;
  load();
};

// Function to remove a class room with confirmation and error handling
const removeClassRoom = async (row) => {
  const id = row.id || row.Id || row.classRoomId || row.ClassRoomId;
  if (!id) return;

  // Determine the label for the class room to display in the confirmation message
  const roomLabel = row.name || row.Name || 'this class room';
  const ok = await confirm({
    title: "Delete class room",
    message: `Are you sure you want to delete the class room "${roomLabel}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) return;

  try {
    // Call the API to delete the class room by ID
    await classRoomApi.delete(id);
    notify.success("Class room deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>