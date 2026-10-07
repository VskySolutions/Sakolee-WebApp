<template>
  <q-page padding>
    <!-- Create is retired here — students are created via Family registration. -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'All Student' }]"
      title="All Students"
      description="Centralized management for all active students."
      :search="search"
      show-search
      search-placeholder="Search name or email"
      show-filters
      :filter-count="filterChips.length"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-select v-model="filters.active" :options="activeFilterOptions" label="Status" />
    </app-filter-drawer>

    <app-data-table
      page-key="students"
      row-key="studentId"
      title="All Students"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      selectable
      @request="onRequest"
      @refresh="load"
      @update:selected="selected = $event"
    >
      <template #bulk-actions="{ selected: sel }">
        <q-btn
          v-if="canWrite" flat dense no-caps color="primary" icon="o_forward_to_inbox"
          label="Send Credentials" :loading="sendingCredentials" @click="sendCredentials(sel)"
        />
      </template>

      <!-- <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">{{ cell.value ? "Active" : "Inactive" }}</q-badge>
        </q-td>
      </template> -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <div class="flex flex-center">
            <q-toggle
              :model-value="cell.row.active ?? cell.row.Active ?? cell.row.isActive ?? cell.row.IsActive ?? true"
              @update:model-value="(val) => updateStatus(cell.row, val)"
              dense
              color="positive"
              :disable="!canWrite"
            />
          </div>
        </q-td>
      </template>

      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn type="a" flat round dense color="primary" icon="o_visibility" @click="openView(cell.row.studentId)">
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <!-- Edit is retired here too — see the header note above. -->
          <q-btn v-if="canWrite" type="a" flat round dense color="primary" icon="o_forward_to_inbox" @click="sendCredentials([cell.row])">
            <q-tooltip>Send Credentials</q-tooltip>
          </q-btn>
          <q-btn v-if="canDelete" type="a" flat round dense color="negative" icon="o_delete" @click="remove(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <!-- Send Credentials: students whose email could NOT be sent, with their temporary passwords so they
         can be shared manually. Successfully emailed passwords are not shown. -->
    <q-dialog v-model="credentialsFailedOpen" persistent>
      <q-card style="min-width: 420px;">
        <q-card-section class="row items-center q-gutter-sm">
          <q-icon name="o_key" color="warning" size="sm" />
          <div class="text-h6">Credentials not emailed</div>
        </q-card-section>
        <q-card-section>
          <div class="text-body2 text-grey-7 q-mb-sm">
            These emails could not be sent. The passwords will not be shown again — share them securely.
          </div>
          <div v-for="f in credentialsFailed" :key="f.email" class="q-mb-sm">
            <div class="text-caption text-grey-7">{{ f.name }} — {{ f.email }}</div>
            <q-input :model-value="f.password" readonly outlined dense>
              <template #append>
                <q-btn flat round dense icon="o_content_copy" @click="copyPassword(f.password)">
                  <q-tooltip>Copy</q-tooltip>
                </q-btn>
              </template>
            </q-input>
          </div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn v-close-popup flat no-caps color="primary" label="Done" />
        </q-card-actions>
      </q-card>
    </q-dialog>
    <view-student :id="viewingId" v-model="viewOpen" />
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { debounce } from "quasar";
import { studentApi, getApiErrorMessage } from "services/api";
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
import ViewStudent from "modules/student/components/view_student.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const auditColumns = useAuditColumns();
const { formatDate } = useDateFormat();
const { has } = usePermissions();

const canDelete = computed(() => has(Permissions.StudentsDelete));
const canWrite = computed(() => has(Permissions.StudentsWrite));

const columns = [
  { name: "firstName", label: "First Name", field: "firstName", align: "left", default: true },
  { name: "lastName", label: "Last Name", field: "lastName", align: "left", default: true },
  { name: "email", label: "Email", field: (row) => row.email || "—", align: "left", default: true },
  { name: "cellPhone", label: "Cell Phone", field: (row) => row.cellPhone || "—", align: "left" },
  { name: "school", label: "School", field: (row) => row.school || "—", align: "left", sortable: true },
  { name: "gradeLevel", label: "Grade", field: (row) => row.gradeLevel || "—", align: "left" },
  { name: "admissionDate", label: "Admission Date", field: (row) => formatDate(row.admissionDate), sort: (row) => row.admissionDate || "", align: "left", sortable: true },

  ...auditColumns(),
  { name: "active", label: "Status", field: "active", align: "center", sortable: true, default: true },
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

const filters = reactive({ active: null });
const { rows, loading, totalRecords, selected, search, filterOpen, pagination, load, onRequest } = useListTable({
  pageKey: "students",
  fetcher: ({ page, limit, sortBy, descending }) =>
    studentApi.list({
      page,
      limit,
      sortBy,
      descending,
      active: filters.active === null ? undefined : filters.active,
      search: search.value || undefined
    }).then((r) => ({ data: r?.data, total: r?.meta?.totalRecords })),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch([search, filters], reload, { deep: true });

const activeFilterOptions = [
  { label: "Active", value: true },
  { label: "Inactive", value: false }
];

const filterChips = computed(() => {
  const chips = [];

  if (filters.active !== null) {
    chips.push({
      key: "active",
      label: `Status: ${filters.active ? "Active" : "Inactive"}`
    });
  }

  return chips;
});

const removeFilter = (key) => {
  if (key === "active") {
    filters.active = null;
  }
};

const clearFilters = () => {
  filters.active = null;
};

/*
 * ------------------------------------------------------------
 * View Student
 * ------------------------------------------------------------
 */
const viewOpen = ref(false);
const viewingId = ref(null);

const openView = (studentId) => {
  viewingId.value = studentId;
  viewOpen.value = true;
};

// ---- Send Credentials ----
const sendingCredentials = ref(false);
const credentialsFailedOpen = ref(false);
const credentialsFailed = ref([]);

const studentName = (s) => `${s.firstName || ""} ${s.lastName || ""}`.trim() || s.email || "Student";

const copyPassword = async (password) => {
  try {
    await navigator.clipboard.writeText(password);
    notify.success("Copied to clipboard.");
  } catch {
    notify.warning("Copy failed — please select and copy manually.");
  }
};

// Used by both the row button (one student) and the bulk action (selected students).
const sendCredentials = async (students) => {
  if (!students.length) return;
  const ok = await confirm({
    title: "Send credentials",
    message: `Email ${students.length === 1 ? studentName(students[0]) : `${students.length} students`} ` +
      "their login credentials (username and temporary password)? Anyone who has already set their own " +
      "password gets a new temporary password and their sessions end.",
    confirmLabel: "Send",
    type: "primary"
  });
  if (!ok) return;

  sendingCredentials.value = true;
  let sent = 0;
  const failed = [];   // email didn't go out — password shown for manual sharing
  const errors = [];   // API call itself failed (no login account, permission, ...)
  try {
    // One at a time: each call sends over SMTP synchronously.
    for (const student of students) {
      try {
        const result = await studentApi.sendCredentials(student.studentId);
        if (result?.emailSent) {
          sent++;
        } else {
          failed.push({ name: studentName(student), email: result?.email || student.email, password: result?.temporaryPassword || "" });
        }
      } catch (err) {
        errors.push(`${studentName(student)}: ${getApiErrorMessage(err)}`);
      }
    }
  } finally {
    sendingCredentials.value = false;
  }

  if (sent) notify.success(`Credentials emailed to ${sent} student(s).`);
  if (errors.length) notify.error(`Could not send credentials — ${errors.join("; ")}`);
  if (failed.length) {
    notify.warning(`${failed.length} email(s) could not be sent (check the tenant's SMTP account).`);
    credentialsFailed.value = failed;
    credentialsFailedOpen.value = true;
  }
  selected.value = [];
};

/*
 * ------------------------------------------------------------
 * Delete
 * ------------------------------------------------------------
 */
// const remove = async (row) => {
//   const ok = await confirm({
//     title: "Delete student",
//     message: `Delete "${row.firstName} ${row.lastName}"?`,
//     confirmLabel: "Delete",
//     type: "danger"
//   });

//   if (!ok) {
//     return;
//   }

//   try {
//     await studentApi.remove(row.studentId);
//     notify.success("Student deleted.");
//     load();
//   } catch (err) {
//     notify.error(getApiErrorMessage(err));
//   }
// };
const remove = async (row) => {
  const name = `${row.firstName || ""} ${row.lastName || ""}`.trim();
  const ok = await confirm({
    title: "Delete student",
    message: `Delete "${name}"? They will be removed from their family and all class enrollments.`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;
  try {
    const result = await studentApi.remove(row.studentId);
    const classes = result?.classesRemoved ?? 0;
    const parts = [`Student "${name}" deleted`];
    if (row.familyName) parts.push(`removed from family "${row.familyName}"`);
    if (classes > 0) parts.push(`removed from ${classes} class(es)`);
    notify.success(parts.join(", ") + ".");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const updateStatus = async (row, newStatus) => {
  const id = row.studentId;
  if (!id) return;

  const originalStatus = row.active;
  row.active = newStatus;

  try {
    await studentApi.update(id, { ...row, active: newStatus });
    notify.success("Student active status updated successfully.");
    await load();
  } catch (err) {
    row.active = originalStatus;
    notify.error(getApiErrorMessage(err));
  }
};
</script>
