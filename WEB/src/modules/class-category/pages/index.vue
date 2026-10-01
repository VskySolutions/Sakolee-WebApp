<template>
  <q-page padding>
    <!-- Page header with breadcrumbs, search, and create button -->
    <app-list-header
      :breadcrumbs="[ { label: 'Home', icon: 'o_home', to: '/' }, { label: 'Class Categories' }]"
      title="Class Categories"
      description="Manage your class categories here."
      :search="search"
      show-search
      search-placeholder="Search name or category type"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canWrite"
      add-label="Create Class Category"
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
      <!-- Show deleted -->
      <q-toggle
        v-if="canManageDeleted"
        v-model="showDeleted"
        label="Show deleted?"
        dense
        class="q-mt-md"
      />
    </app-filter-drawer>
    <!-- Class Category table -->
    <app-data-table
      page-key="class-categories"
      row-key="classCategoryId"
      title="Class Categories"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Category Type -->
      <template #body-cell-categoryType="cell">
        <q-td :props="cell">
          <q-badge color="primary">
            {{ cell.value || "—" }}
          </q-badge>
        </q-td>
      </template>
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
              {{
                cell.row.active
                  ? "Active"
                  : "Inactive"
              }}
            </q-tooltip>
          </q-toggle>
        </q-td>
      </template>
      <!-- Action buttons -->
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
            @click="openEdit(cell.row)"
          >
            <q-tooltip>Edit</q-tooltip>
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
            @click="deleteCategory(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <!-- Create / Edit Dialog -->
    <class-category-form
      v-model="formOpen"
      :editing="editing"
      :category="selectedCategory"
      @saved="load"
    />
    <!-- View Class Category -->
    <app-form-dialog
      v-model="viewOpen"
      title="View Class Category"
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
            {{ viewCategory.name || "—" }}
          </div>
        </div>
        <!-- Tenant -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Tenant
          </div>
          <div class="text-2e fs-14">
            {{ viewCategory.tenantName || "—" }}
          </div>
        </div>
        <!-- Created By -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewCategory.createdBy || "—" }}
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
                viewCategory.createdOnUtc
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
            {{ viewCategory.updatedBy || "—" }}
          </div>
        </div>
        <!-- Updated On -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{ viewCategory.updatedBy ? formatDateTime( viewCategory.updatedOnUtc ) : "—" }}
          </div>
        </div>
        <!-- Category Type -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Category Type
          </div>
          <div class="text-2e fs-14">
            {{ viewCategory.categoryType || "—" }}
          </div>
        </div>
        <!-- Status -->
        <div class="col-12 col-sm-6">
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge :class=" viewCategory.active ? 'active-badge' : 'inactive-badge'">{{ viewCategory.active ? "Active" : "Inactive" }}</q-badge>
        </div>
      </div>
    </app-form-dialog>
  </q-page>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import { date, debounce } from "quasar";
import { classCategoryApi, getApiErrorMessage } from "services/api";
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
import ClassCategoryForm from "modules/class-category/components/ClassCategoryForm.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
const { showDeleted, canManageDeleted } = useDeletedRecords();
/*
 * Permissions
 */
const canRead = computed(() =>
  has("classes.read")
);
const canWrite = computed(() =>
  has("classes.write")
);
const canDelete = computed(() =>
  has("classes.delete")
);
/*
 * Format UTC date values for display.
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
 *
 * Category Type and Status are placed
 * at the end, similar to Hear About Us.
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
    format: (val) =>
      formatDateTime(val)
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
  /*
   * Category Type is near the end.
   */
  {
    name: "categoryType",
    label: "Category Type",
    field: "categoryType",
    align: "left",
    sortable: true,
    default: true,
    filterable: true,
    filterOptions: [
      {
        label: "Category 1",
        value: "Category 1"
      },
      {
        label: "Category 2",
        value: "Category 2"
      },
      {
        label: "Category 3",
        value: "Category 3"
      }
    ]
  },
  /*
   * Status is immediately before Actions.
   */
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
 * Server-side filters.
 *
 * Filters:
 * - Name
 * - Category Type
 * - Status
 */
const {
  filters,
  filterableColumns,
  filterChips,
  removeFilter,
  clearFilters
} = useColumnFilters(
  columns,
  ref([]),
  {
    server: true
  }
);
/*
 * Class Category table.
 */
const {
  rows,
  loading,
  totalRecords,
  search,
  pagination,
  load,
  onRequest
} = useListTable({
  pageKey: "class-categories",
  fetcher: ({ page, limit, sortBy, descending }) => classCategoryApi.list({ page, limit, search: search.value || undefined, sortBy, descending, name: filters.name || undefined, categoryType: filters.categoryType || undefined, active: filters.active ?? undefined, showDeleted: showDeleted.value })
    .then((response) => ({ data: response?.data || [], total: response?.meta?.totalRecords || 0 })),
  onError: (error) => {
    notify.error(getApiErrorMessage(error, "Unable to load class categories."));
  }
});

/*
 * Reload the table when:
 * - Search changes
 * - Name filter changes
 * - Category Type filter changes
 * - Status filter changes
 * - Show Deleted changes
 */
const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);
watch([search, showDeleted, filters], reload, { deep: true });
/*
 * Create / Edit state.
 */
const formOpen = ref(false);
const editing = ref(false);
const selectedCategory = ref(null);
/*
 * View state.
 */
const viewOpen = ref(false);
const viewLoading = ref(false);
const viewCategory = ref({
  classCategoryId: null,
  name: "",
  categoryType: "",
  active: true,
  deleted: false,
  tenantName: "",
  createdBy: null,
  createdOnUtc: null,
  updatedBy: null,
  updatedOnUtc: null
});
/*
 * Reset view data.
 */
const resetViewCategory = () => {
  viewCategory.value = {
    classCategoryId: null,
    name: "",
    categoryType: "",
    active: true,
    deleted: false,
    tenantName: "",
    createdBy: null,
    createdOnUtc: null,
    updatedBy: null,
    updatedOnUtc: null
  };
};
/*
 * Open View dialog.
 */
const openView = async (row) => {
  resetViewCategory();
  viewOpen.value = true;
  viewLoading.value = true;
  try {
    const category = await classCategoryApi.get(row.classCategoryId);
    viewCategory.value = {
      classCategoryId: category?.classCategoryId,
      name: category?.name || "",
      categoryType: category?.categoryType || "",
      active: category?.active ?? true,
      deleted: category?.deleted ?? false,
      tenantName: category?.tenantName || "",
      createdBy: category?.createdBy || "",
      createdOnUtc: category?.createdOnUtc || null,
      updatedBy: category?.updatedBy || "",
      updatedOnUtc: category?.updatedOnUtc || null
    };
  } catch (error) {
    viewOpen.value = false;
    notify.error(getApiErrorMessage(error, "Unable to load class category."));
  } finally {
    viewLoading.value = false;
  }
};
/*
 * Close View dialog.
 */
const closeView = () => {
  viewOpen.value = false;
  resetViewCategory();
};
/*
 * Open Create dialog.
 */
const openCreate = () => {
  selectedCategory.value = null;
  editing.value = false;
  formOpen.value = true;
};
/*
 * Open Edit dialog.
 */
const openEdit = async (row) => {
  try {
    const category =
      await classCategoryApi.get(row.classCategoryId);
    selectedCategory.value = category;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    notify.error(
      getApiErrorMessage(error, "Unable to load class category.")
    );
  }
};

/*
 * Toggle Active / Inactive.
 */
const toggleActive = async (row, active) => {
  const previousValue = row.active;
  // Update the table immediately.
  row.active = active;
  try {
    await classCategoryApi.update(row.classCategoryId, { name: row.name, categoryType: row.categoryType, active });
    notify.success(active ? "Class category activated." : "Class category deactivated.");
  } catch (error) {
    // Restore the previous value if update fails.
    row.active = previousValue;
    notify.error(getApiErrorMessage(error, "Unable to update class category status."));
  }
};
/*
 * Delete Class Category.
 */
const deleteCategory = async (row) => {
  const confirmed = await confirm({
    title: "Delete Class Category",
    message: `Delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!confirmed) {
    return;
  }
  try {
    await classCategoryApi.remove(row.classCategoryId);
    notify.success("Class category deleted.");
    load();
  } catch (error) {
    notify.error(
      getApiErrorMessage(error, "Unable to delete class category.")
    );
  }
};
/*
 * Initial load.
 */
load();
</script>
