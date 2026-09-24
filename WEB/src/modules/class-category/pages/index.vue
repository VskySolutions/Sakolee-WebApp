<template>
  <q-page padding>
    <!-- Page header with breadcrumbs, search, and create button -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Class Categories' }
      ]"
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
      title="All Class Categories"
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
        <q-td :props="cell" text-class="text-center">
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
     <!-- Dialog used to display Class Category details -->
    <q-dialog
      v-model="viewOpen"
      position="right"
      transition-show="slide-left"
      transition-hide="slide-right"
    >
      <q-card
        class="column no-wrap"
        style="
          width: 350px;
          max-width: 100vw;
          max-height: 70vh;
        "
      >
      <!-- Dialog header -->
        <q-card-section class="row items-center">
          <div class="text-h6 text-primary">
            Class Category Details
          </div>
          <q-space />
           <!-- Close details dialog -->
          <q-btn
            icon="o_close"
            flat
            round
            dense
            v-close-popup
          />
        </q-card-section>
        <q-separator />
        <!-- Display selected category details -->
        <q-card-section class="col scroll">
          <q-list>
            <q-item
              v-for="field in viewFields"
              :key="field.label"
            >
              <q-item-section>
                <q-item-label caption>
                  {{ field.label }}
                </q-item-label>
              </q-item-section>
              <q-item-section side>
                <!-- Display value as a badge when required -->
                <q-item-label v-if="field.badge">
                  <q-badge color="primary">
                    {{ field.value }}
                  </q-badge>
                </q-item-label>
                 <!-- Display normal text value -->
                <q-item-label v-else>
                  {{ field.value }}
                </q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </q-card-section>
      </q-card>
    </q-dialog>
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
    name: "createdOnUtc",
    label: "Created On",
    field: "createdOnUtc",
    align: "left",
    sortable: true,
    default: true,
    format: (val) => formatDateTime(val)
  },
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "left"
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
const viewed = ref(null);

// Prepare fields to display in the view dialog.
const viewFields = computed(() =>
  viewed.value
    ? [
      { label: "Name", value: viewed.value.name },
      {
        label: "Category Type",
        value: viewed.value.categoryType || "—",
        badge: !!viewed.value.categoryType
      },
      { label: "Tenant", value: viewed.value.tenantName || "—" },
      { label: "Created On", value: formatDateTime(viewed.value.createdOnUtc) }
    ]
    : []
);

// Open the view dialog for the selected category.
const openView = (row) => {
  viewed.value = row;
  viewOpen.value = true;
};

// const editFromView = () => {
//   const row = viewed.value;
//   viewOpen.value = false;
//   openEdit(row);
// };

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
