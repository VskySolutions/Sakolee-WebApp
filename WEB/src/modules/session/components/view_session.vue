<template>
  <app-form-dialog
    v-model="isOpen"
    title="View Session"
    size="sm"
    hide-save
    @cancel="close"
  >
    <!-- Loading State Spinner -->
    <div v-if="viewLoading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <!-- Content Section -->
    <div v-else class="row q-col-gutter-lg">
      <!-- Session Name -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Session Name</div>
        <div class="text-2e fs-14">
          {{ viewSession.sessionName || "—" }}
        </div>
      </div>

      <!-- Status -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Status</div>
        <q-badge :class="viewSession.isActive ? 'active-badge' : 'inactive-badge'">
          {{ viewSession.isActive ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Created By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created By</div>
        <div class="text-2e fs-14">
          {{ viewSession.createdBy || "—" }}
        </div>
      </div>

      <!-- Created On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewSession.createdOnUtc) }}
        </div>
      </div>

      <!-- Updated By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated By</div>
        <div class="text-2e fs-14">
          {{ viewSession.updatedBy || "—" }}
        </div>
      </div>

      <!-- Updated On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewSession.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { classSessionApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";

// Props and emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  recordId: { type: [Object, String, Number], default: null }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const isOpen = ref(props.modelValue);
const viewLoading = ref(false);

// Reactive object to hold the session data for viewing
const viewSession = reactive({
  id: null,
  sessionName: "",
  isActive: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

// Utility function to format date
// const formatDate = (value) => {
//   if (!value) return "—";
//   const date = new Date(value);
//   return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
// };

const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  // Check if date is invalid or the default .NET MinValue (0001-01-01)
  if (isNaN(date.getTime()) || date.getFullYear() <= 1) return "—";
  return date.toLocaleString();
};

// Function to reset the view state
const resetView = () => {
  viewSession.id = null;
  viewSession.sessionName = "";
  viewSession.isActive = true;
  viewSession.createdBy = "";
  viewSession.createdOnUtc = null;
  viewSession.updatedBy = "";
  viewSession.updatedOnUtc = null;
};

// Watchers to handle prop changes and open/close state
watch(() => props.modelValue, async (val) => {
  isOpen.value = val;
  if (val && props.recordId) {
    await fetchRecord(props.recordId);
  } else if (!val) {
    resetView();
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

// Function to fetch a record by ID
const fetchRecord = async (id) => {
  resetView();
  viewLoading.value = true;
  try {
    const response = await classSessionApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewSession.id = id;
      viewSession.sessionName = item.sessionName || item.SessionName || item.name || item.Name || "";
      viewSession.isActive = item.isActive ?? item.IsActive ?? item.active ?? item.Active ?? true;
      viewSession.createdBy = item.createdBy || item.CreatedBy || item.created_by || "";
      viewSession.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
      viewSession.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
      viewSession.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
    }
  } catch (err) {
    isOpen.value = false;
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
};

const close = () => {
  isOpen.value = false;
  resetView();
};
</script>