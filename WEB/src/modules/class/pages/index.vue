<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' }, { label: 'Classes' }]"
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
      @add="openCreate"
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
          <q-btn v-if="canWrite" type="a" flat round dense color="primary" icon="o_edit" @click="openEdit(cell.row)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn v-if="canDelete" type="a" flat round dense color="negative" icon="o_delete" @click="remove(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit drawer -->
    <app-form-drawer
      v-model="formOpen"
      :title="editing ? 'Edit Class' : 'Add Class'"
      :saving="saving"
      @submit="submitForm"
      @cancel="resetForm"
    >
      <q-form ref="formRef" greedy>
        <div class="row q-col-gutter-md">
          <div class="col-12 form-section-title">Class Information</div>
          <app-text-field v-model="form.className" label="Class Name *" placeholder="e.g. Advanced Ballet" class="col-12 col-sm-6" :rules="[(v) => !!v || 'Class name is required']" />

          <!-- Category/Location/Room/Session/Instructor: placeholder option lists only — there is no
               Category/Location/Room/Session/Instructor management feature yet, so these don't save to
               the class record until that backing data exists. -->
          <app-select v-model="form.category1" label="Category 1 *" :options="category1Options" class="col-12 col-sm-6" />
          <app-select v-model="form.category2" label="Category 2" :options="category2Options" class="col-12 col-sm-6" />
          <app-select v-model="form.category3" label="Category 3" :options="category3Options" class="col-12 col-sm-6" />
          <app-select v-model="form.location" label="Location *" :options="locationOptions" class="col-12 col-sm-6" />
          <app-select v-model="form.room" label="Room *" :options="roomOptions" class="col-12 col-sm-6" />
          <app-select v-model="form.session" label="Session *" :options="sessionOptions" class="col-12 col-sm-6" />
          <app-select v-model="form.primaryInstructor" label="Primary Instructor *" :options="instructorOptions" class="col-12 col-sm-6" />
          <app-text-field v-model="form.additionalInstructors" label="Additional Instructors (Max 2)" hint="Free text (no instructor list yet)" class="col-12 col-sm-6" />

          <q-separator class="col-12 q-my-sm" />
          <div class="col-12 form-section-title">Timeframe</div>
          <app-date-field v-model="form.startDate" label="Start Date *" class="col-12 col-sm-6" />
          <app-date-field v-model="form.endDate" label="End Date *" class="col-12 col-sm-6" />
          <app-date-field v-model="form.registrationOpenDate" label="Registration Open Date *" class="col-12 col-sm-6" />

          <div class="col-12">
            <app-field-label label="Active Days" />
            <div class="row q-gutter-sm">
              <q-btn
                v-for="day in dayOptions" :key="day.value"
                :label="day.label" rounded dense no-caps unelevated
                :color="isDaySelected(day.value) ? 'primary' : 'grey-3'"
                :text-color="isDaySelected(day.value) ? 'white' : 'grey-8'"
                class="day-toggle"
                @click="toggleDay(day.value)"
              />
            </div>
          </div>

          <app-text-field v-model="form.startTime" label="Start Time *" placeholder="e.g. 04:42 AM" class="col-12 col-sm-6">
            <template #append><q-icon name="o_schedule" size="20px" color="grey-6" /></template>
          </app-text-field>
          <app-text-field v-model="form.endTime" label="End Time *" placeholder="e.g. 06:44 AM" class="col-12 col-sm-6">
            <template #append><q-icon name="o_schedule" size="20px" color="grey-6" /></template>
          </app-text-field>
          <app-text-field v-model="form.duration" label="Duration" placeholder="e.g. 1 hour 00 minutes" class="col-12 col-sm-6" />

          <q-separator class="col-12 q-my-sm" />
          <div class="col-12 form-section-title">Pricing</div>
          <app-text-field v-model.number="form.tuitionFee" label="Tuition Fee *" type="number" placeholder="0.00" class="col-12 col-sm-6" :rules="[(v) => v !== null && v !== '' || 'Tuition fee is required']">
            <template #prepend><span class="text-grey-7">$</span></template>
          </app-text-field>
          <app-select v-model="form.billingMethod" label="Billing Method" :options="billingMethodOptions" class="col-12 col-sm-6" />
          <app-select v-model="form.billingCycle" label="Billing Cycle" :options="billingCycleOptions" class="col-12 col-sm-6" />
          <div class="col-12 col-sm-6 toggle-row-inline">
            <q-toggle v-model="form.registrationFee" color="primary" />
            <span class="q-ml-sm">Registration Fee Required</span>
          </div>

          <q-separator class="col-12 q-my-sm" />
          <div class="col-12 form-section-title">Additional Information</div>
          <app-text-field v-model="form.description" label="Description" type="textarea" placeholder="Enter class description..." class="col-12" />
          <div class="col-12">
            <app-field-label label="Gender" />
            <q-option-group
              v-model="form.gender" :options="genderOptions" type="radio" inline dense color="primary"
              class="gender-options"
            />
          </div>
          <app-text-field v-model.number="form.minAge" label="Min Age" type="number" placeholder="Min Age" class="col-12 col-sm-6" />
          <app-text-field v-model.number="form.maxAge" label="Max Age" type="number" placeholder="Max Age" class="col-12 col-sm-6" />
          <app-text-field v-model.number="form.maxClassSize" label="Max Class Size" type="number" placeholder="Max Class Size" class="col-12 col-sm-6" />
          <app-text-field v-model.number="form.maxWaitlistSize" label="Max Waitlist Size" type="number" placeholder="Max Waitlist Size" class="col-12 col-sm-6" />
          <app-date-field v-model="form.cutoffDate" label="Cutoff Date" class="col-12 col-sm-6" />
          <app-text-field v-model="form.policyGroups" label="Policy Groups" hint="Free text (no policy group list yet)" class="col-12 col-sm-6" />

          <q-separator class="col-12 q-my-sm" />
          <div class="col-12 form-section-title">Media</div>
          <app-text-field v-model="form.virtualClassUrl" label="Virtual Class URL" placeholder="https://zoom.us/j/..." class="col-12 col-sm-6" />
          <app-text-field v-model="form.linkDisplayText" label="Link Display Text" placeholder="Join Virtual Class" class="col-12 col-sm-6" />

          <q-separator class="col-12 q-my-sm" />
          <div class="col-12 form-section-title">Additional Settings</div>
          <div class="col-12 col-sm-6 toggle-row"><span>Online Listings</span><q-toggle v-model="form.onlineListings" color="primary" /></div>
          <div class="col-12 col-sm-6 toggle-row"><span>Parent Portal Schedule</span><q-toggle v-model="form.parentPortalSchedule" color="primary" /></div>
          <div class="col-12 col-sm-6 toggle-row"><span>Online Registration</span><q-toggle v-model="form.onlineRegistration" color="primary" /></div>
          <div class="col-12 col-sm-6 toggle-row"><span>Makeups in Class</span><q-toggle v-model="form.makeupsInClass" color="primary" /></div>
          <div class="col-12 col-sm-6 toggle-row"><span>Allow Waitlist In Roll (Legacy Form)</span><q-toggle v-model="form.allowWaitlistInRoll" color="primary" /></div>
          <div class="col-12 col-sm-6 toggle-row"><span>Allow Waitlist Enrollment (Parent Portal)</span><q-toggle v-model="form.allowWaitlistEnrollment" color="primary" /></div>
          <div class="col-12 col-sm-6 toggle-row"><span>Allow Portal Enrollment</span><q-toggle v-model="form.allowPortalEnrollment" color="primary" /></div>
          <div class="col-12 col-sm-6 toggle-row"><span>Allow Portal Drop Requests</span><q-toggle v-model="form.allowPortalDropRequests" color="primary" /></div>
          <div class="col-12 col-sm-6 toggle-row"><span>Allow Drop-Ins</span><q-toggle v-model="form.allowDropIns" color="primary" /></div>
          <div class="col-12 col-sm-6 toggle-row"><span>Drop-In Fee</span><q-toggle v-model="form.dropInFee" color="primary" /></div>

          <template v-if="editing">
            <q-separator class="col-12 q-my-sm" />
            <div class="col-12 toggle-row-inline">
              <q-toggle v-model="form.active" color="primary" />
              <span class="q-ml-sm">Active</span>
            </div>
          </template>
        </div>
      </q-form>
    </app-form-drawer>
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { debounce } from "quasar";
import { classApi, classCategoryApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useAuditColumns } from "composables/useAuditColumns";
import { useDateFormat } from "composables/useDateFormat";
import { usePermissions, Permissions } from "composables/usePermissions";

import AppDataTable from "components/common/AppDataTable.vue";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppDateField from "components/common/AppDateField.vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";

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

// ---- Create / Edit ----
// LocationId/RoomId/SessionId/PrimaryInstructorId are not on this form — there is no Location/Room/
// Session/Instructor management feature yet to pick one from (same reason Student's ParentId/ClassId/
// FeeCategoryId aren't on its form either).
const formOpen = ref(false);
const editing = ref(false);
const saving = ref(false);
const formRef = ref(null);
const blankForm = () => ({
  classId: null,
  className: "",
  // Placeholder-only until Category/Location/Room/Session/Instructor management exists — not part of
  // the payload in submitForm.
  category1: "",
  category2: "",
  category3: "",
  location: "",
  room: "",
  session: "",
  primaryInstructor: "",
  additionalInstructors: "",
  gender: "",
  minAge: null,
  maxAge: null,
  description: "",
  startDate: "",
  endDate: "",
  registrationOpenDate: "",
  cutoffDate: "",
  activeDays: "",
  duration: "",
  startTime: "",
  endTime: "",
  tuitionFee: null,
  billingMethod: "",
  billingCycle: "",
  registrationFee: false,
  dropInFee: false,
  maxClassSize: null,
  maxWaitlistSize: null,
  allowWaitlistInRoll: false,
  allowWaitlistEnrollment: false,
  allowDropIns: false,
  makeupsInClass: false,
  virtualClassUrl: "",
  linkDisplayText: "",
  onlineListings: false,
  onlineRegistration: false,
  allowPortalEnrollment: false,
  parentPortalSchedule: false,
  allowPortalDropRequests: false,
  policyGroups: "",
  active: true
});
const form = reactive(blankForm());

// ---- Field options ----
// Category 1/2/3: real options loaded from ClassCategory (scoped to the caller's active tenant), one
// flat list told apart by categoryType. Location/Room/Session/Instructor stay demo option lists
// standing in for the not-yet-built management features (see the Class entity's own comment on
// LocationId/RoomId/SessionId/PrimaryInstructorId). None of these are sent on submit — see
// submitForm's payload.
const classCategories = ref([]);
const loadClassCategories = async () => {
  try {
    classCategories.value = await classCategoryApi.list() || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
onMounted(loadClassCategories);

const categoryOptions = (categoryType) => computed(() =>
  classCategories.value
    .filter((c) => c.categoryType === categoryType)
    .map((c) => ({ label: c.name, value: c.classCategoryId })));
const category1Options = categoryOptions("Category 1");
const category2Options = categoryOptions("Category 2");
const category3Options = categoryOptions("Category 3");
const locationOptions = ["North Studio", "Downtown Campus", "Westside Academy"];
const roomOptions = ["Studio A", "Studio B", "Main Gym"];
const sessionOptions = ["Spring 2024", "Fall 2024", "Winter 2025"];
const instructorOptions = ["Sarah Jenkins", "Michael Chen", "Elena Rodriguez"];

const genderOptions = [
  { label: "All", value: "All" },
  { label: "Male", value: "Male" },
  { label: "Female", value: "Female" }
];
const billingMethodOptions = ["Flat Rate", "Per Session", "Per Class", "Hourly"];
const billingCycleOptions = ["Monthly", "Weekly", "Bi-Weekly", "Per Session", "One-Time"];

// Active Days stays a comma-separated free-text string on the server (see Class entity); the pill
// toggles below just read/write that string so nothing on the wire changes.
const dayOptions = [
  { label: "Mon", value: "Mon" },
  { label: "Tue", value: "Tue" },
  { label: "Wed", value: "Wed" },
  { label: "Thu", value: "Thu" },
  { label: "Fri", value: "Fri" },
  { label: "Sat", value: "Sat" },
  { label: "Sun", value: "Sun" }
];
const selectedDays = computed(() => (form.activeDays || "").split(",").map((d) => d.trim()).filter(Boolean));
const isDaySelected = (day) => selectedDays.value.includes(day);
const toggleDay = (day) => {
  const days = isDaySelected(day) ? selectedDays.value.filter((d) => d !== day) : [...selectedDays.value, day];
  form.activeDays = dayOptions.map((d) => d.value).filter((d) => days.includes(d)).join(", ");
};

const resetForm = () => {
  Object.assign(form, blankForm());
  editing.value = false;
};

const openCreate = () => {
  resetForm();
  formOpen.value = true;
};

const openEdit = (row) => {
  resetForm();
  editing.value = true;
  Object.assign(form, {
    classId: row.classId,
    className: row.className || "",
    additionalInstructors: row.additionalInstructors || "",
    gender: row.gender || "",
    minAge: row.minAge ?? null,
    maxAge: row.maxAge ?? null,
    description: row.description || "",
    startDate: row.startDate ? row.startDate.substring(0, 10) : "",
    endDate: row.endDate ? row.endDate.substring(0, 10) : "",
    registrationOpenDate: row.registrationOpenDate ? row.registrationOpenDate.substring(0, 10) : "",
    cutoffDate: row.cutoffDate ? row.cutoffDate.substring(0, 10) : "",
    activeDays: row.activeDays || "",
    duration: row.duration || "",
    startTime: row.startTime || "",
    endTime: row.endTime || "",
    tuitionFee: row.tuitionFee ?? null,
    billingMethod: row.billingMethod || "",
    billingCycle: row.billingCycle || "",
    registrationFee: !!row.registrationFee,
    dropInFee: !!row.dropInFee,
    maxClassSize: row.maxClassSize ?? null,
    maxWaitlistSize: row.maxWaitlistSize ?? null,
    allowWaitlistInRoll: !!row.allowWaitlistInRoll,
    allowWaitlistEnrollment: !!row.allowWaitlistEnrollment,
    allowDropIns: !!row.allowDropIns,
    makeupsInClass: !!row.makeupsInClass,
    virtualClassUrl: row.virtualClassUrl || "",
    linkDisplayText: row.linkDisplayText || "",
    onlineListings: !!row.onlineListings,
    onlineRegistration: !!row.onlineRegistration,
    allowPortalEnrollment: !!row.allowPortalEnrollment,
    parentPortalSchedule: !!row.parentPortalSchedule,
    allowPortalDropRequests: !!row.allowPortalDropRequests,
    policyGroups: row.policyGroups || "",
    active: row.active
  });
  formOpen.value = true;
};

const submitForm = async ({ clearDraft } = {}) => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  const payload = {
    className: form.className,
    additionalInstructors: form.additionalInstructors || null,
    gender: form.gender || null,
    minAge: form.minAge,
    maxAge: form.maxAge,
    description: form.description || null,
    startDate: form.startDate || null,
    endDate: form.endDate || null,
    registrationOpenDate: form.registrationOpenDate || null,
    cutoffDate: form.cutoffDate || null,
    activeDays: form.activeDays || null,
    duration: form.duration || null,
    startTime: form.startTime || null,
    endTime: form.endTime || null,
    tuitionFee: form.tuitionFee,
    billingMethod: form.billingMethod || null,
    billingCycle: form.billingCycle || null,
    registrationFee: form.registrationFee,
    dropInFee: form.dropInFee,
    maxClassSize: form.maxClassSize,
    maxWaitlistSize: form.maxWaitlistSize,
    allowWaitlistInRoll: form.allowWaitlistInRoll,
    allowWaitlistEnrollment: form.allowWaitlistEnrollment,
    allowDropIns: form.allowDropIns,
    makeupsInClass: form.makeupsInClass,
    virtualClassUrl: form.virtualClassUrl || null,
    linkDisplayText: form.linkDisplayText || null,
    onlineListings: form.onlineListings,
    onlineRegistration: form.onlineRegistration,
    allowPortalEnrollment: form.allowPortalEnrollment,
    parentPortalSchedule: form.parentPortalSchedule,
    allowPortalDropRequests: form.allowPortalDropRequests,
    policyGroups: form.policyGroups || null
  };

  saving.value = true;
  try {
    if (editing.value) {
      await classApi.update(form.classId, { ...payload, active: form.active });
      notify.success("Class updated.");
    } else {
      await classApi.create(payload);
      notify.success("Class created.");
    }
    clearDraft?.();
    formOpen.value = false;
    resetForm();
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
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

<style scoped>
.form-section-title {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--q-primary);
}
.toggle-row,
.toggle-row-inline {
  display: flex;
  align-items: center;
  min-height: 40px;
}
.toggle-row {
  justify-content: space-between;
}
.day-toggle {
  min-width: 56px;
}
.gender-options :deep(.q-radio) {
  margin-right: 16px;
}
</style>
