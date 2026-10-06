<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Family Relations' }
      ]"
      title="Family Relations"
      description="Manage all family relation types here."
      :search="search"
      show-search
      search-placeholder="Search relations"
    
      :show-add="canWrite"
      add-label="Create Relation"
      show-back
      @update:search="search = $event"
    
      @add="openCreate"
      @back="$router.back()"
    />
    <!--Hiding the filter attribute-->
    <!-- show-filters
      :filter-count="filterChips.length"
      @filters="filterOpen = true" -->

      <!-- Hiding Filter Drawer Component -->
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

    <app-data-table
      page-key="family-relations"
      row-key="id"
      title="Relations"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="rows.length"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
    <!--Hiding the filter attribute-->
     <!-- :rows="filteredRows"
     :total-records="filteredRows.length" -->

      <!-- Status Column with Interactive Toggle
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-toggle
            v-model="cell.row.active"
            :disable="!canWrite"
            dense
            @update:model-value="(val) => toggleStatus(cell.row, val)"
          >
            <q-tooltip>{{ cell.row.active ? 'Active' : 'Inactive' }}</q-tooltip>
          </q-toggle>
        </q-td>
      </template> -->

      <!-- Interactive Status Toggle Column -->
<template #body-cell-active="cell">
  <q-td :props="cell">
     <div class="flex flex-center">
      <q-toggle
        :model-value="cell.row.active ?? cell.row.Active ?? cell.row.is_active ?? true"
        @update:model-value="(val) => toggleStatus(cell.row, val)"
        dense
        color="positive"
        :disable="!canWrite || cell.row.deleted || cell.row.Deleted"
      />
      <!-- <span :class="(cell.row.active ?? cell.row.Active ?? cell.row.is_active ?? true) ? 'text-positive' : 'text-grey'">
        {{ (cell.row.active ?? cell.row.Active ?? cell.row.is_active ?? true) ? "Active" : "Inactive" }}
      </span>-->
    </div> 
  </q-td>
</template>
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
            v-if="canWrite"
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
            v-if="canDelete"
            flat
            round
            dense
            color="negative"
            icon="o_delete"
            @click="removeRelation(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit Form Component -->
    <family-relation-form
      v-model="formDrawerOpen"
      :editing-id="editingId"
      :initial-data="selectedRow"
      @saved="handleSaved"
    />

    <!-- View Component -->
    <family-relation-view
      v-model="viewDrawerOpen"
      :record-id="viewRecordId"
    />
  </q-page>
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { debounce } from "quasar";

import { familyRelationApi, getApiErrorMessage } from "services/api";

import { usePermissions, Permissions } from "composables/usePermissions";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";
import { useAuditColumns } from "composables/useAuditColumns";

import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";

import FamilyRelationForm from "src/modules/family-relations/components/create_edit.vue";
import FamilyRelationView from "src/modules/family-relations/components/view_realtion.vue";

const auditColumns = useAuditColumns();
const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();

const canWrite = computed(() => has(Permissions.FamilyRelationsWrite));
const canDelete = computed(() => has(Permissions.FamilyRelationsDelete));

// Define the columns for the data table
const columns = [
  {
    name: "name",
    label: "Relation Name",
    field: "name",
    align: "left",
    sortable: true,
    default: true,
    filterable: true
    
  },
  
  //...auditColumns(),
  {
    name: "createdBy",
    label: "Created By",
    field: (row) => row.createdBy || row.CreatedBy || "—",
    align: "left",
    sortable: true,
    default: true,
    filterable: false
  },
  {
    name: "createdOnUtc",
    label: "Created On",
    field: (row) => row.createdOnUtc || row.CreatedOnUtc,
    format: (val) => val ? new Date(val).toLocaleString() : "—",
    align: "left",
    sortable: true,
    default: true,
    filterable: false
  },
  {
    name: "updatedBy",
    label: "Updated By",
    field: (row) => row.updatedBy || row.UpdatedBy || "—",
    align: "left",
    sortable: true,
    default: true,
    filterable: false
  },
  {
    name: "updatedOnUtc",
    label: "Updated On",
    field: (row) => row.updatedOnUtc || row.UpdatedOnUtc,
    format: (val) => {
      if (!val) return "—";
      const date = new Date(val);
      // Agar date default value (0001 ya 1900) hai toh "-" show karein
      if (isNaN(date.getTime()) || date.getFullYear() <= 1900) {
        return "—";
      }
      return date.toLocaleString();
    },
    align: "left",
    sortable: true,
    default: true,
    filterable: false
  },
  {
    name: "active",
    label: "Status",
    field: "active",
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

// List table setup with fetcher containing showDeleted parameter
const {
  rows,
  loading,
  search,
  pagination,
  load,
  onRequest
} = useListTable({
  server: false,
  pageKey: "family-relations",
  defaultSortBy: "createdOnUtc",
  defaultDescending: true,
  fetcher: ({ sortBy, descending }) => {
    const activeSortBy = sortBy || pagination.value.sortBy || "createdOnUtc";
    const activeDescending = descending ?? pagination.value.descending ?? true;

    return familyRelationApi.list({
      search: search.value || undefined,
      showDeleted: showDeleted.value,// Added showDeleted query parameter
      sortBy: activeSortBy,
      descending: activeDescending
    }).then((response) => {
      let items = response?.data || [];

      items.sort((a, b) => {
        let valA = a[activeSortBy];
        let valB = b[activeSortBy];

        if (activeSortBy === 'createdOnUtc' || !activeSortBy) {
          const dateA = new Date(a.createdOnUtc || a.created_on_utc || 0).getTime();
          const dateB = new Date(b.createdOnUtc || b.created_on_utc || 0).getTime();
          if (dateA !== dateB) {
            return activeDescending ? dateB - dateA : dateA - dateB;
          }
          return activeDescending ? (b.id - a.id) : (a.id - b.id);
        }

        if (typeof valA === 'string' && typeof valB === 'string') {
          const comp = valA.localeCompare(valB);
          return activeDescending ? -comp : comp;
        }

        if (valA < valB) return activeDescending ? 1 : -1;
        if (valA > valB) return activeDescending ? -1 : 1;
        return 0;
      });

      return {
        data: items,
        total: items.length
      };
    });
  },
  onError: (err) => notify.error(getApiErrorMessage(err))
});

// Function to update status directly from table toggle in database
const toggleStatus = async (row, newStatus) => {
  try {
    await familyRelationApi.update(row.id, {
      ...row,
      active: newStatus
    });
    notify.success("Status updated successfully.");
    await load();
  } catch (err) {
    // Revert state on failure
    row.active = !newStatus;
    notify.error(getApiErrorMessage(err));
  }
};

const handleSaved = async () => {
  pagination.value.page = 1;
  pagination.value.sortBy = "createdOnUtc";
  pagination.value.descending = true;
  await load();
};

onMounted(() => {
  pagination.value.sortBy = "createdOnUtc";
  pagination.value.descending = true;
  load();
});

// Commented filters Funcationality
// Filter Drawer State
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
// Watcher for showDeleted toggle to reload table data automatically
watch(showDeleted, () => {
  pagination.value.page = 1;
  load();
});

// Form Drawer State
const formDrawerOpen = ref(false);
const editingId = ref(null);
const selectedRow = ref(null);

const openCreate = () => {
  editingId.value = null;
  selectedRow.value = null;
  formDrawerOpen.value = true;
};

const openEdit = (row) => {
  editingId.value = row.id;
  selectedRow.value = row;
  formDrawerOpen.value = true;
};

// View Drawer State
const viewDrawerOpen = ref(false);
const viewRecordId = ref(null);

const openView = (row) => {
  viewRecordId.value = row.id;
  viewDrawerOpen.value = true;
};

// Deletion Workflow
const removeRelation = async (row) => {
  const ok = await confirm({
    title: "Delete family relation",
    message: `Are you sure you want to delete the relation "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) return;

  try {
    await familyRelationApi.delete(row.id);
    notify.success("Family relation deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>