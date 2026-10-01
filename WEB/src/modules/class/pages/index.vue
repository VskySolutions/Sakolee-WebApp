<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'All Classes' }]"
      title="All Classes"
      description="Manage all classes."
      :search="search"
      show-search
      search-placeholder="Search class name"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canWrite"
      add-label="Add Class"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="$router.push({ name: 'class_create' })"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-select v-model="filters.active" :options="activeFilterOptions" label="Status" />
    </app-filter-drawer>

    <app-data-table
      page-key="classes"
      row-key="classId"
      title="All Classes"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">{{ cell.value ? "Active" : "Inactive" }}</q-badge>
        </q-td>
      </template>

      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" :to="{ name: 'class_detail', params: { id: cell.row.classId } }">
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <q-btn v-if="canWrite" flat round dense color="primary" icon="o_edit" :to="{ name: 'class_edit', params: { id: cell.row.classId } }">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn v-if="canDelete" flat round dense color="negative" icon="o_delete" @click="remove(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
  </q-page>
</template>

<script setup>
import { computed, reactive, watch } from "vue";
import { debounce } from "quasar";
import { classApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useAuditColumns } from "composables/useAuditColumns";
import { useDateFormat } from "composables/useDateFormat";
import { usePermissions, Permissions } from "composables/usePermissions";

import AppDataTable from "components/common/AppDataTable.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppSelect from "components/common/AppSelect.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const auditColumns = useAuditColumns();
const { formatDate } = useDateFormat();
const { has } = usePermissions();
const canWrite = computed(() => has(Permissions.ClassesWrite));
const canDelete = computed(() => has(Permissions.ClassesDelete));

const columns = [
  { name: "className", label: "Class Name", field: "className", align: "left", sortable: true, default: true },
  { name: "startDate", label: "Start Date", field: (row) => formatDate(row.startDate), sort: (row) => row.startDate || "", align: "left", sortable: true, default: true },
  { name: "endDate", label: "End Date", field: (row) => formatDate(row.endDate), sort: (row) => row.endDate || "", align: "left", sortable: true },
  { name: "startTime", label: "Start Time", field: (row) => row.startTime || "—", align: "left" },
  { name: "endTime", label: "End Time", field: (row) => row.endTime || "—", align: "left" },
  { name: "tuitionFee", label: "Tuition Fee", field: (row) => row.tuitionFee ?? "—", align: "left" },
  { name: "maxClassSize", label: "Max Size", field: (row) => row.maxClassSize ?? "—", align: "left" },
  { name: "active", label: "Status", field: "active", align: "left", sortable: true, default: true },
  ...auditColumns(),
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

const filters = reactive({ active: null });
const { rows, loading, totalRecords, search, filterOpen, pagination, load, onRequest } = useListTable({
  pageKey: "classes",
  fetcher: ({ page, limit, sortBy, descending }) =>
    classApi.list({
      page,
      limit,
      sortBy,
      descending,
      active: filters.active === null ? undefined : filters.active,
      search: search.value || undefined
    })
      .then((r) => ({ data: r?.data, total: r?.meta?.totalRecords })),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

const reload = debounce(() => { pagination.value.page = 1; load(); }, 300);
watch([search, filters], reload, { deep: true });

const activeFilterOptions = [{ label: "Active", value: true }, { label: "Inactive", value: false }];

const filterChips = computed(() => {
  const chips = [];
  if (filters.active !== null) chips.push({ key: "active", label: `Status: ${filters.active ? "Active" : "Inactive"}` });
  return chips;
});

const removeFilter = (key) => {
  if (key === "active") filters.active = null;
};
const clearFilters = () => {
  filters.active = null;
};

const remove = async (row) => {
  const ok = await confirm({
    title: "Delete class",
    message: `Delete "${row.className}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;
  try {
    await classApi.remove(row.classId);
    notify.success("Class deleted.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>
