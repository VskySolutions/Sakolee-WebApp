<template>
  <q-page padding>
    <!-- Page header with breadcrumbs, search, and create button -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Class Categories' }
      ]"
      title="Class Categories"
      description="Manage your class categories here."
      :search="search"
      show-search
      search-placeholder="Search name or category type"
      :show-add="canWrite"
      add-label="Create Class Category"
      show-back
      @update:search="search = $event"
      @add="openCreate"
      @back="$router.back()"
    />
    <!-- Table displaying all Class Categories -->
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
     <!-- Display Category Type as a badge -->
      <template #body-cell-categoryType="cell">
        <q-td :props="cell">
          <q-badge color="primary">
            {{ cell.value }}
          </q-badge>
        </q-td>
      </template>
      <!-- Action buttons for viewing, editing, and deleting -->
      <template #body-cell-actions="cell">
         <q-td :props="cell" text-class="text-center" style="padding-right: 150px !important;">
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
            @click="deleteCategory(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
     <!-- Drawer used for creating and editing Class Categories -->
    <class-category-form
      v-model="formOpen"
      :editing="editing"
      :category="selectedCategory"
      @saved="load"
    />
    <!-- View Class Category -->
    <app-form-drawer
      v-model="viewOpen"
      title="View Class Category"
      :saving="viewLoading"
      :save-label="''"
      :hide-save="true"
      @cancel="closeView"
    >
      <div class="q-gutter-md">
        <div>
          <div class="text-86 fs-12 fw-500">
            Name
          </div>
          <div class="text-2e fs-4">
            {{ viewCategory.name || "—" }}
          </div>
        </div>
        <div>
          <div class="text-86 fs-12 fw-500">
            Category Type
          </div>
          <div class="text-2e fs-4">
            {{ viewCategory.categoryType || "—" }}
          </div>
        </div>
        <div>
          <div class="text-86 fs-12 fw-500">
            Tenant
          </div>
          <div class="text-2e fs-4">
            {{ viewCategory.tenantName || "—" }}
          </div>
        </div>
        <div>
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewCategory.createdBy || "—" }}
          </div>
        </div>
        <div>
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{ formatDateTime(viewCategory.createdOnUtc) }}
          </div>
        </div>
        <div>
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewCategory.updatedBy || "—" }}
          </div>
        </div>
        <div>
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{ formatDateTime(viewCategory.updatedOnUtc) }}
          </div>
        </div>
      </div>
    </app-form-drawer>
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
import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import ClassCategoryForm from "modules/class-category/components/ClassCategoryForm.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
const canRead = computed(() => has("classes.read"));
const canWrite = computed(() => has("classes.write"));
const canDelete = computed(() => has("classes.delete"));

// Format UTC date values for display.
const formatDateTime = (value) => {
  if (!value) return "—";
  // DateTime values from SQL Server can arrive without a "Z", and JS would then
  // read them as local time. Force UTC so the browser converts to the user's timezone.
  const iso = /(Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : `${value}Z`;
  return date.formatDate(new Date(iso), "MM/DD/YYYY hh:mm A");
};

// Define the columns displayed in the Class Category table.
const columns = [
  {
    name: "name",
    label: "Name",
    field: "name",
    align: "left",
    sortable: true,
    default: true
  },
  {
    name: "categoryType",
    label: "Category Type",
    field: "categoryType",
    align: "left",
    sortable: true,
    default: true
  },
  {
    name: "tenantName",
    label: "Tenant",
    field: "tenantName",
    align: "left",
    sortable: true,
    default: true
  },
   {
    name: "createdBy",
    label: "Created By",
    field: "createdBy",
    align: "left",
    sortable: true,
    default: true,
    format: (val) => val || "—"
  },
  {
    name: "createdOnUtc",
    label: "Created On",
    field: "createdOnUtc",
    align: "left",
    sortable: true,
    default: true,
    format: (val) => formatDateTime(val)
  },
  {
    name: "updatedBy",
    label: "Updated By",
    field: "updatedBy",
    align: "left",
    sortable: true,
    default: true,
    format: (val) => val || "—"
  },
  {
    name: "updatedOnUtc",
    label: "Updated On",
    field: "updatedOnUtc",
    align: "left",
    sortable: true,
    default: true,
    format: (val) => formatDateTime(val)
  },
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "right",
    style: "padding-right: 150px !important;",
    headerStyle: "padding-right: 190px !important;"
  }
];

// Configure the reusable list table functionality.
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
  // Fetch Class Categories from the API.
  fetcher: ({ page, limit, sortBy, descending }) =>
    classCategoryApi
      .list({
        search: search.value || undefined
      })
      .then((response) => {
        // Make sure the API response contains an array
        const data = Array.isArray(response?.data)
          ? response.data
          : [];
        // Return the table data and total record count.
        return {
          data,
          total: data.length
        };
      }),
  // Display an error when loading categories fails.
  onError: (error) => {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load class categories."
      )
    );
  }
});
// Reload the table when the search value changes.
// Debounce prevents an API call for every keystroke.
const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);
watch(search, reload);

const formOpen = ref(false);
const editing = ref(false);
const selectedCategory = ref(null);
const viewOpen = ref(false);
const viewLoading = ref(false);
// Define the structure of the category being viewed.
const viewCategory = ref({
  classCategoryId: null,
  name: "",
  categoryType: "",
  tenantName: "",
  createdBy: null,
  createdOnUtc: null,
  updatedBy: null,
  updatedOnUtc: null
});
// Reset the viewCategory to its initial state.
const resetViewCategory = () => {
  viewCategory.value = {
    classCategoryId: null,
    name: "",
    categoryType: "",
    tenantName: "",
    createdBy: null,
    createdOnUtc: null,
    updatedBy: null,
    updatedOnUtc: null
  };
};
// Open the view dialog for the selected Class Category and load its details.
const openView = async (row) => {
  // Reset the viewCategory to ensure no stale data is displayed.
  resetViewCategory();
  viewOpen.value = true;
  viewLoading.value = true;
  try {
    const category = await classCategoryApi.get(
      row.classCategoryId
    );
    // Set the viewCategory with the fetched data for display.
    viewCategory.value = {
      classCategoryId: category?.classCategoryId,
      name: category?.name || "",
      categoryType: category?.categoryType || "",
      tenantName: category?.tenantName || "",
      createdBy: category?.createdBy || "",
      createdOnUtc: category?.createdOnUtc || null,
      updatedBy: category?.updatedBy || "",
      updatedOnUtc: category?.updatedOnUtc || null
    };
  } catch (error) {
    viewOpen.value = false;
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load class category."
      )
    );
  } finally {
    viewLoading.value = false;
  }
};
// Load Class Categories when the page is opened.
const closeView = () => {
  viewOpen.value = false;
  resetViewCategory();
};
// Open the form for creating a new category.
const openCreate = () => {
  selectedCategory.value = null;
  editing.value = false;
  formOpen.value = true;
};

// Load the selected category and open the edit form.
const openEdit = async (row) => {
  try {
    const category = await classCategoryApi.get(
      row.classCategoryId
    );
      // Set the category data and enable edit mode.
    selectedCategory.value = category;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    // Show an error if the category cannot be loaded.
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load class category."
      )
    );
  }
};

// Delete the selected Class Category.
const deleteCategory = async (row) => {
  const confirmed = await confirm({
    title: "Delete Class Category",
    message: `Delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  // Stop if the user cancels the operation.
  if (!confirmed) {
    return;
  }
  try {
    // Delete the category using the API.
    await classCategoryApi.remove(
      row.classCategoryId
    );
    notify.success("Class category deleted.");
    // Reload the table with the latest data.
    load();
  } catch (error) {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to delete class category."
      )
    );
  }
};
// Load Class Categories when the page is opened.
load();
</script>
