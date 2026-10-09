<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Families', to: '/families' },
        { label: 'Lead Files' }
      ]"
      title="All Leads"
      description="Students not currently enrolled in a running class - a lead line for follow-up."
      show-filters
      :filter-count="filterChips.length"
      show-back
      @filters="filterOpen = true"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-column-filters v-model="filters" :columns="filterableColumns" />
    </app-filter-drawer>

    <app-data-table
      page-key="leads"
      row-key="studentId"
      title="All Leads"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <template #body-cell-familyName="cell">
        <q-td :props="cell">
          <button
            v-if="cell.row.familyId"
            type="button"
            class="family-cell row items-center no-wrap q-gutter-x-sm"
            @click="openFamilyView(cell.row)"
          >
            <span class="family-cell__avatar">{{ familyInitials(cell.row.familyName) }}</span>
            <span class="family-cell__name">{{ cell.row.familyName || "—" }}</span>
          </button>
          <span v-else>—</span>
        </q-td>
      </template>
    </app-data-table>

    <family-view-drawer v-model="viewOpen" :family-id="viewFamilyId" />
  </q-page>
</template>

<script setup>
import { computed, ref, watch, onMounted } from "vue";
import { leadApi, locationApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDateFormat } from "composables/useDateFormat";
import { debounce } from "quasar";

import AppDataTable from "components/common/AppDataTable.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import FamilyViewDrawer from "modules/family/components/FamilyViewDrawer.vue";

const notify = useNotify();
const fmt = useDateFormat();

// Studio Location filter options, loaded once from the Locations list (id → name).
const locationOptions = ref([]);
const loadLocations = async () => {
  try {
    const response = await locationApi.list({ limit: 100, sortBy: "name", descending: false });
    locationOptions.value = (response?.data || []).map((l) => ({ label: l.name, value: l.id }));
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
onMounted(loadLocations);

// A column's `name` doubles as its filter's query parameter. Display columns follow the reference
// layout (Location / Family / Contact Name / Email / Phone / Student Phone / Date Created); the name
// columns above are kept as hidden-but-filterable columns so the server-side name filters still work.
const columns = computed(() => [
  {
    name: "studioLocationId",
    label: "Location",
    field: (row) => row.studioLocationName || "—",
    align: "left",
    sortable: true,
    default: true,
    ...(locationOptions.value.length ? { filterOptions: locationOptions.value } : { filterable: false })
  },
  { name: "familyName", label: "Family", field: "familyName", align: "left", sortable: true, filterable: false, default: true },
  { name: "contactName", label: "Contact Name", field: (row) => [row.contactFirstName, row.contactLastName].filter(Boolean).join(" ") || "—", align: "left", filterable: false, default: true },
  { name: "email", label: "Email", field: (row) => row.contactEmail || row.email || "—", align: "left", sortable: true, default: true },
  { name: "contactPhone", label: "Phone", field: (row) => row.contactPhone || "—", align: "left", filterable: false, default: true },
  { name: "cellPhone", label: "Student Phone", field: (row) => row.cellPhone || "—", align: "left", filterable: false, default: true },
  { name: "createdOnUtc", label: "Date Created", field: (row) => fmt.formatDate(row.createdOnUtc), align: "left", sortable: true, filterable: false, default: true },
  { name: "studentFirstName", label: "Student Name", field: (row) => row.studentFirstName || "—", align: "left", sortable: true, default: false },
  { name: "studentLastName", label: "Student Last Name", field: (row) => row.studentLastName || "—", align: "left", sortable: true, default: false },
  { name: "contactFirstName", label: "Contact First Name", field: (row) => row.contactFirstName || "—", align: "left", sortable: true, default: false },
  { name: "contactLastName", label: "Contact Last Name", field: (row) => row.contactLastName || "—", align: "left", sortable: true, default: false },
  { name: "admissionDate", label: "Admission Date", field: (row) => fmt.formatDate(row.admissionDate), align: "left", sortable: true, default: false }
]);

// Table state and data fetching
const { rows, loading, totalRecords, filterOpen, pagination, load, onRequest } = useListTable({
  pageKey: "leads",
  defaultSortBy: "studentLastName",
  fetcher: ({ page, limit, sortBy, descending }) =>
    leadApi.list({
      page,
      limit,
      sortBy,
      descending,
      studioLocationId: filters.studioLocationId || undefined,
      studentFirstName: filters.studentFirstName || undefined,
      studentLastName: filters.studentLastName || undefined,
      contactFirstName: filters.contactFirstName || undefined,
      contactLastName: filters.contactLastName || undefined,
      email: filters.email || undefined
    }).then((r) => ({ data: r?.data, total: r?.meta?.totalRecords })),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

const { filters, filterableColumns, filterChips, removeFilter, clearFilters } = useColumnFilters(columns, rows, { server: true });
const reload = debounce(() => { pagination.value.page = 1; load(); }, 300);
watch(filters, reload, { deep: true });

// ---- Read-only Family view (FamilyViewDrawer) ----
const viewOpen = ref(false);
const viewFamilyId = ref(null);
const openFamilyView = (row) => {
  viewFamilyId.value = row.familyId;
  viewOpen.value = true;
};

const familyInitials = (name) => {
  if (!name) return "?";
  return name
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part.charAt(0).toUpperCase())
    .join("");
};
</script>

<style scoped lang="scss">
.family-cell {
  margin: 0;
  padding: 0;
  border: none;
  background: none;
  font: inherit;
  color: inherit;
  cursor: pointer;

  &:hover .family-cell__name {
    text-decoration: underline;
  }
}

.family-cell__avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: #eef1f6;
  color: #5b6b83;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  flex: 0 0 auto;
}

.family-cell__name {
  color: #2c6e9e;
  font-weight: 600;
  white-space: nowrap;
}
</style>
