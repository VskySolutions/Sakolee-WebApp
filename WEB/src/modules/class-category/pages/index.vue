<template>
  <q-page padding>
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
      <template #body-cell-categoryType="cell">
        <q-td :props="cell">
          <q-badge color="primary">
            {{ cell.value }}
          </q-badge>
        </q-td>
      </template>

      <template #body-cell-actions="cell">
        <q-td :props="cell" text-class="text-center">
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

    <class-category-form
      v-model="formOpen"
      :editing="editing"
      :category="selectedCategory"
      @saved="load"
    />
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
        <q-card-section class="row items-center">
          <div class="text-h6 text-primary">
            Class Category Details
          </div>

          <q-space />

          <q-btn
            icon="o_close"
            flat
            round
            dense
            v-close-popup
          />
        </q-card-section>

        <q-separator />

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
                <q-item-label v-if="field.badge">
                  <q-badge color="primary">
                    {{ field.value }}
                  </q-badge>
                </q-item-label>

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

import {
  classCategoryApi,
  getApiErrorMessage
} from "services/api";

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

const formatDateTime = (value) => {
  if (!value) return "—";

  // DateTime values from SQL Server can arrive without a "Z", and JS would then
  // read them as local time. Force UTC so the browser converts to the user's timezone.
  const iso = /(Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : `${value}Z`;

  return date.formatDate(new Date(iso), "MM/DD/YYYY hh:mm A");
};

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

  fetcher: ({ page, limit, sortBy, descending }) =>
    classCategoryApi
      .list({
        search: search.value || undefined
      })
      .then((response) => {
        const data = Array.isArray(response?.data)
          ? response.data
          : [];

        return {
          data,
          total: data.length
        };
      }),

  onError: (error) => {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load class categories."
      )
    );
  }
});

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

const openEdit = async (row) => {
  try {
    const category = await classCategoryApi.get(
      row.classCategoryId
    );

    selectedCategory.value = category;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load class category."
      )
    );
  }
};

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
    await classCategoryApi.remove(
      row.classCategoryId
    );

    notify.success("Class category deleted.");

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

load();
</script>
