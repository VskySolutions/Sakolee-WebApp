<template>
  <app-form-dialog
    v-model="isOpen"
    title="View Location"
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
      <!-- Location Name -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Location Name</div>
        <div class="text-2e fs-14">
          {{ viewLocation.name || "—" }}
        </div>
      </div>

      <!-- Status -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Status</div>
        <q-badge :class="viewLocation.active ? 'active-badge' : 'inactive-badge'">
          {{ viewLocation.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Created By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created By</div>
        <div class="text-2e fs-14">
          {{ viewLocation.createdBy || "—" }}
        </div>
      </div>

      <!-- Created On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewLocation.createdOnUtc) }}
        </div>
      </div>

      <!-- Updated By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated By</div>
        <div class="text-2e fs-14">
          {{ viewLocation.updatedBy || "—" }}
        </div>
      </div>

      <!-- Updated On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewLocation.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { locationApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";

// Props and Emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  recordId: { type: [Object, String, Number], default: null }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const isOpen = ref(props.modelValue);
const viewLoading = ref(false);

// Reactive object to hold the location data for viewing
const viewLocation = reactive({
  id: null,
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

// // Utility function to format date
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
  viewLocation.id = null;
  viewLocation.name = "";
  viewLocation.active = true;
  viewLocation.createdBy = "";
  viewLocation.createdOnUtc = null;
  viewLocation.updatedBy = "";
  viewLocation.updatedOnUtc = null;
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
    const response = await locationApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewLocation.id = id;
      viewLocation.name = item.name || item.Name || item.locationName || item.LocationName || "";
      viewLocation.active = item.active ?? item.Active ?? item.is_active ?? true;
      viewLocation.createdBy = item.createdBy || item.CreatedBy || item.created_by || "";
      viewLocation.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
      viewLocation.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
      viewLocation.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
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