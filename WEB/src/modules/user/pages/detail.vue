<template>
  <app-form-dialog
    v-model="isOpen"
    :title="staff ? staff.fullName : 'Staff Details'"
    :subtitle="staffSubtitle"
    avatar-text="S"
    size="xxl"
    hide-save
    hide-footer
    class="view-class-scss"
  >
    <q-tabs
      v-model="tab"
      dense
      class="text-grey"
      active-color="primary"
      indicator-color="primary"
      align="justify"
      narrow-indicator no-caps
    >
      <q-tab name="summary" label="Summary" />
      <q-tab name="class-list" label="Classes" />
      <!-- <q-tab name="drop-list" label="Drop List" />
      <q-tab name="absent-list" label="Absent List" /> -->
    </q-tabs>
    <q-tab-panels v-model="tab" animated class="bg-fb">
      <q-tab-panel name="summary">
        <div v-if="staffLoading" class="row flex-center q-pa-xl">
          <q-spinner color="primary" size="32px" />
        </div>

        <q-tab-panel v-else-if="staff" class="view-student-scss">
          <!-- SUMMARY CARDS-->
          <div class="student-summary row q-col-gutter-md q-mb-md">
            <div class="col-12">
              <div class="summary-card">
                <div class="summary-card__label">ACCOUNT STATUS</div>
                <div class="summary-card__value">
                  <q-badge :color="staff.isActive ? 'positive' : 'grey'">
                    {{ staff.isActive ? "Active" : "Inactive" }}
                  </q-badge>
                </div>
              </div>
            </div>
          </div>

          <!-- PERSONAL / GENERAL INFORMATION -->
          <div class="row q-col-gutter-md">
            <!-- Personal / General Info -->
            <div class="col-12">
              <section class="border-80 br-16 pa-20">
                <div class="info-card__header">
                  <q-icon name="o_badge" class="fs-16 text-4d" />
                  <span class="text-4d fw-700 fs-12 lh-16">STAFF INFORMATION</span>
                </div>

                <div class="mt-12">
                  <div class="info-row">
                    <span class="fs-11 fw-400 text-86">Full Name</span>
                    <span class="fs-11 fw-600 text-2e">{{ staff.fullName || "—" }}</span>
                  </div>

                  <div class="info-row">
                    <span class="fs-11 fw-400 text-86">Email</span>
                    <span class="fs-11 fw-600 text-2e">{{ staff.email || "—" }}</span>
                  </div>
                </div>
              </section>
            </div>
          </div>

          <!--AUDIT -->
          <div class="mt-24">
            <app-record-audit :audit="staff.audit" />
          </div>
        </q-tab-panel>
      </q-tab-panel>
      <q-tab-panel name="class-list" class="view-class-enrollment-list-scss">
        <app-data-table
          page-key="staff-classes"
          row-key="classId"
          :rows="rows"
          :columns="columns"
          :loading="classesLoading"
          :total-records="totalRecords"
          :pagination="pagination"
          @request="onRequest"
          @refresh="loadClasses"
        />
      </q-tab-panel>
    </q-tab-panels>
  </app-form-dialog>
</template>

<script setup>

// Import necessary modules and components

import { ref, computed, watch } from "vue";
import { userApi, classApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useDateFormat } from "composables/useDateFormat";
import { useListTable } from "composables/useListTable";

import AppFormDialog from "components/common/AppFormDialog.vue";
import AppRecordAudit from "components/common/AppRecordAudit.vue";
import AppDataTable from "components/common/AppDataTable.vue";

// Props for the component
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  userId: { type: String, default: null }
});
const emit = defineEmits(["update:modelValue"]);

const { formatLongDate } = useDateFormat();
const notify = useNotify();
const staff = ref(null);
const staffLoading = ref(false);

// Computed property to manage the open state of the dialog
const isOpen = computed({
  get: () => props.modelValue,
  set: (v) => emit("update:modelValue", v)
});

// Computed property for the subtitle of the staff profiles
const staffSubtitle = computed(() => {
  return "Staff Member Profile & Details";
});

// Function to load staff details based on the provided userId
const loadStaff = async () => {
  if (!props.userId) return;
  staffLoading.value = true;
  try {
    staff.value = await userApi.get(props.userId);
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    staffLoading.value = false;
  }
};

const tab = ref("summary");

const columns = [
  { name: "locationName", label: "Locaction", field: (row) => row.locationName || "—", align: "left", sortable: true, default: true },
  { name: "className", label: "Class", field: (row) => row.className || "—", align: "left", sortable: true, default: true },
  { name: "status", label: "Status", field: "status", align: "left", sortable: true, default: true },
  { name: "sessionName", label: "Session", field: (row) => row.sessionName || "—", align: "left", sortable: true, default: true },
  { name: "currentEnrollment", label: "Current Enroll", field: "currentEnrollment", align: "center", sortable: true, default: true },
  { name: "waitList", label: "Wait List", field: "waitList", align: "center", sortable: true, default: true },
  { name: "roomName", label: "Room", field: (row) => row.roomName || "—", align: "left", sortable: true, default: true },
  { name: "startDate", label: "Start Date", field: (row) => row.startDate ? formatLongDate(row.startDate) : "—", align: "left", sortable: true, default: true },
  { name: "endDate", label: "End Date", field: (row) => row.endDate ? formatLongDate(row.endDate) : "—", align: "left", sortable: true, default: true },
  { name: "days", label: "Days", field: (row) => row.days || "—", align: "left", sortable: true, default: true },
  { name: "times", label: "Times", field: (row) => row.times || "—", align: "left", sortable: true, default: true }
];

const { rows, loading: classesLoading, totalRecords, pagination, load: loadClasses, onRequest } = useListTable({
  pageKey: "staff-classes",
  fetcher: ({ page, limit, sortBy, descending }) => {
    if (!props.userId) {
      return Promise.resolve({
        data: [],
        total: 0
      });
    }

    return classApi
      .listByInstructor(props.userId, {
        page,
        limit,
        sortBy,
        descending
      })
      .then((r) => ({
        data: r?.data || [],
        total: r?.meta?.totalRecords || 0
      }));
  },

  onError: (err) => {
    notify.error(getApiErrorMessage(err));
  }
});

watch(tab, (value) => {
  if (value === "class-list" && props.userId) {
    loadClasses();
  }
});

watch(
  () => props.userId,
  () => {
    if (props.modelValue) {
      loadStaff();

      if (tab.value === "class-list") {
        loadClasses();
      }
    }
  }
);

watch(
  () => props.modelValue,
  (value) => {
    if (value) {
      loadStaff();

      if (tab.value === "class-list") {
        loadClasses();
      }
    }
  }
);

// watch(
//   () => tab.value === "class-list" && props.modelValue,
//   (value) => {
//     if (value) {
//       tab.value = "summary";
//     }
//   }
// );
</script>
