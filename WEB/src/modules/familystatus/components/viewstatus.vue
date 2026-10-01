<template>
  <app-form-dialog
    v-model="isOpen"
    title="View Family Status"
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
      <!-- Family Status Name -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Family Status Name</div>
        <div class="text-2e fs-14">
          {{ viewFamilyStatus.familyStatusName || "—" }}
        </div>
      </div>

      <!-- Status -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Status</div>
        <q-badge :class="viewFamilyStatus.active ? 'active-badge' : 'inactive-badge'">
          {{ viewFamilyStatus.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Created By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created By</div>
        <div class="text-2e fs-14">
          {{ viewFamilyStatus.createdBy || "—" }}
        </div>
      </div>

      <!-- Created On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewFamilyStatus.createdOnUtc) }}
        </div>
      </div>

      <!-- Updated By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated By</div>
        <div class="text-2e fs-14">
          {{ viewFamilyStatus.updatedBy || "—" }}
        </div>
      </div>

      <!-- Updated On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewFamilyStatus.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { familyStatusApi, getApiErrorMessage } from "services/api";
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

// Reactive object to hold the family status data for viewing
const viewFamilyStatus = reactive({
  id: null,
  familyStatusName: "",
  active: true,
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
  viewFamilyStatus.id = null;
  viewFamilyStatus.familyStatusName = "";
  viewFamilyStatus.active = true;
  viewFamilyStatus.createdBy = "";
  viewFamilyStatus.createdOnUtc = null;
  viewFamilyStatus.updatedBy = "";
  viewFamilyStatus.updatedOnUtc = null;
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
    const response = await familyStatusApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewFamilyStatus.id = id;
      viewFamilyStatus.familyStatusName = item.familyStatusName || item.FamilyStatusName || item.name || item.Name || "";
      viewFamilyStatus.active = item.active ?? item.Active ?? item.is_active ?? true;
      viewFamilyStatus.createdBy = item.createdBy || item.CreatedBy || item.created_by || "";
      viewFamilyStatus.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
      viewFamilyStatus.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
      viewFamilyStatus.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
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