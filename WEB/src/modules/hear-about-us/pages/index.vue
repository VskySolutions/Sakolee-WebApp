<template>
  <q-page padding>
    <!-- Page header with breadcrumbs, search, and create button -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Hear About Us' }
      ]"
      title="Hear About Us"
      description="Manage your Hear About Us here."
      :search="search"
      show-search
      search-placeholder="Search Hear About Us name"
      
      :show-add="canWrite"
      add-label="Create Hear About Us"
      show-back
      @update:search="search = $event"
      @add="openCreate"
      @back="$router.back()"
    />
    <!--Hiding the filter attribute-->
    <!-- show-filters
      :filter-count="filterChips.length"
      @filters="filterOpen = true" -->
    <!-- Filter drawer for filtering Hear About Us records -->
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
    <!-- Table displaying all Hear About Us records -->
    <app-data-table
      page-key="hear-about-us"
      row-key="hearAboutUsId"
      title="Hear About Us"
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
            @click="deleteHearAboutUs(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <!-- Drawer used for creating and editing Hear About Us records -->
    <create-edit
      v-model="formOpen"
      :editing="editing"
      :hear-about-us="selectedHearAboutUs"
      @saved="load"
    />
    <!-- View Hear About Us -->
    <app-form-dialog
      v-model="viewOpen"
      title="View Hear About Us"
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
            {{ viewHearAboutUs.name || "—" }}
          </div>
        </div>
        <!-- Status -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge
            :class="
              viewHearAboutUs.active
                ? 'active-badge'
                : 'inactive-badge'
            "
          >
            {{ viewHearAboutUs.active ? "Active" : "Inactive" }}
          </q-badge>
        </div>
        <!-- Tenant -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Tenant
          </div>
          <div class="text-2e fs-14">
            {{ viewHearAboutUs.tenantName || "—" }}
          </div>
        </div>
        <!-- Created By -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewHearAboutUs.createdBy || "—" }}
          </div>
        </div>
        <!-- Created On -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{ formatDateTime(viewHearAboutUs.createdOnUtc) }}
          </div>
        </div>
        <!-- Updated By -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewHearAboutUs.updatedBy || "—" }}
          </div>
        </div>
        <!-- Updated On -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{
              viewHearAboutUs.updatedBy
                ? formatDateTime(viewHearAboutUs.updatedOnUtc)
                : "—"
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
import { hearAboutUsApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { usePermissions } from "composables/usePermissions";
import { useListTable } from "composables/useListTable";
import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFormDialog from "components/common/AppFormDialog.vue";
import CreateEdit from "modules/hear-about-us/components/CreateEdit.vue";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
const { showDeleted, canManageDeleted } = useDeletedRecords();
// Check permissions for Hear About Us actions.
const canRead = computed(() =>
  has("hearAboutUs.read")
);
// Check permissions for Hear About Us actions.
const canWrite = computed(() =>
  has("hearAboutUs.write")
);
// Check permissions for Hear About Us actions.
const canDelete = computed(() =>
  has("hearAboutUs.delete")
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
// Define the columns displayed in the Hear About Us table.
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
const filterOpen = ref(false);
// Configure the reusable list table.
const { rows, loading, totalRecords, search, pagination, load, onRequest } = useListTable({
  pageKey: "hear-about-us",
  // Fetch Hear About Us records from the API
  // with optional search and sorting.
  fetcher: ({ page, limit, sortBy, descending }) => hearAboutUsApi.list({ page, limit, search: search.value || undefined, sortBy, descending, name: filters.name || undefined, active: filters.active ?? undefined, showDeleted: showDeleted.value})
    .then((response) => ({ data: response?.data || [], total: response?.meta?.totalRecords || 0 })),
  // Handle API errors.
  onError: (error) => {
    notify.error(getApiErrorMessage(error, "Unable to load Hear About Us records."));
  }
});
// Configure server-side filters.
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
// Create/Edit drawer state.
const formOpen = ref(false);
const editing = ref(false);
const selectedHearAboutUs = ref(null);
// View drawer state.
const viewOpen = ref(false);
const viewLoading = ref(false);
// Define the structure of the Hear About Us record being viewed.
const viewHearAboutUs = ref({
  hearAboutUsId: null,
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
const resetViewHearAboutUs = () => {
  viewHearAboutUs.value = {
    hearAboutUsId: null,
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
// Open the View drawer for the selected record.
const openView = async (row) => {
  resetViewHearAboutUs();
  viewOpen.value = true;
  viewLoading.value = true;
  // Fetch the latest record from the API.
  try {
    const hearAboutUs =
      await hearAboutUsApi.get(
        row.hearAboutUsId
      );
    viewHearAboutUs.value = {
      hearAboutUsId: hearAboutUs?.hearAboutUsId,
      name: hearAboutUs?.name || "",
      tenantName: hearAboutUs?.tenantName || "",
      createdBy: hearAboutUs?.createdBy || "",
      createdOnUtc: hearAboutUs?.createdOnUtc || null,
      updatedBy: hearAboutUs?.updatedBy || "",
      updatedOnUtc: hearAboutUs?.updatedOnUtc || null,
      active: hearAboutUs?.active ?? true,
      deleted: hearAboutUs?.deleted ?? false
    };
  } catch (error) {
    viewOpen.value = false;
    notify.error(getApiErrorMessage(error, "Unable to load Hear About Us record."));
  } finally {
    viewLoading.value = false;
  }
};
// Close the View drawer and reset its state.
const closeView = () => {
  viewOpen.value = false;
  resetViewHearAboutUs();
};
// Open the form for creating a new record.
const openCreate = () => {
  selectedHearAboutUs.value = null;
  editing.value = false;
  formOpen.value = true;
};
// Load the selected record and open the edit form.
const openEdit = async (row) => {
  try {
    const hearAboutUs = await hearAboutUsApi.get(
      row.hearAboutUsId
    );
    selectedHearAboutUs.value = hearAboutUs;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load Hear About Us record."
      )
    );
  }
};
const toggleActive = async (row, active) => {
  const previousValue = row.active;
  // Update UI immediately.
  row.active = active;
  try {
    await hearAboutUsApi.update(row.hearAboutUsId, {
      name: row.name,
      active
    });
    notify.success(
      active
        ? "Hear About Us activated."
        : "Hear About Us deactivated."
    );
  } catch (error) {
    // Restore previous value if update fails.
    row.active = previousValue;
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to update Hear About Us status."
      )
    );
  }
};
// Delete the selected Hear About Us record.
const deleteHearAboutUs = async (row) => {
  const confirmed = await confirm({
    title: "Delete Hear About Us",
    message: `Delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  // Stop if the user cancels the operation.
  if (!confirmed) {
    return;
  }
  try {
    await hearAboutUsApi.remove(row.hearAboutUsId);
    notify.success("Hear About Us deleted.");
    load();
  } catch (error) {
    notify.error(getApiErrorMessage(error, "Unable to delete Hear About Us record."));
  }
};
// Load Hear About Us records when the page opens.
load();
</script>
