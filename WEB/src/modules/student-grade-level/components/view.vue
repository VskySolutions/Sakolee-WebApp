<template>
  <app-form-dialog
    v-model="isOpen"
    title="View Student Grade Level"
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
      <!-- Student Grade Level Name -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Student Grade Level Name</div>
        <div class="text-2e fs-14">
          {{ viewStudentGradeLevel.name || "—" }}
        </div>
      </div>

      <!-- Status -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Status</div>
        <q-badge :class="viewStudentGradeLevel.active ? 'active-badge' : 'inactive-badge'">
          {{ viewStudentGradeLevel.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Created By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created By</div>
        <div class="text-2e fs-14">
          {{ viewStudentGradeLevel.createdBy || "—" }}
        </div>
      </div>

      <!-- Created On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewStudentGradeLevel.createdOnUtc) }}
        </div>
      </div>

      <!-- Updated By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated By</div>
        <div class="text-2e fs-14">
          {{ viewStudentGradeLevel.updatedBy || "—" }}
        </div>
      </div>

      <!-- Updated On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewStudentGradeLevel.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { studentGradeLevelApi, getApiErrorMessage } from "services/api";
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

// Reactive object to hold the student grade level data for viewing
const viewStudentGradeLevel = reactive({
  id: null,
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

// Enhanced date formatting to handle invalid dates and .NET MinValue
const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  // Check if date is invalid or the default .NET MinValue (0001-01-01)
  if (isNaN(date.getTime()) || date.getFullYear() <= 1) return "—";
  return date.toLocaleString();
};

// Function to reset the view state
const resetView = () => {
  viewStudentGradeLevel.id = null;
  viewStudentGradeLevel.name = "";
  viewStudentGradeLevel.active = true;
  viewStudentGradeLevel.createdBy = "";
  viewStudentGradeLevel.createdOnUtc = null;
  viewStudentGradeLevel.updatedBy = "";
  viewStudentGradeLevel.updatedOnUtc = null;
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
    const response = await studentGradeLevelApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewStudentGradeLevel.id = id;
      viewStudentGradeLevel.name = item.name || item.Name || item.studentGradeLevelName || item.StudentGradeLevelName || "";
      viewStudentGradeLevel.active = item.active ?? item.Active ?? item.is_active ?? true;
      viewStudentGradeLevel.createdBy = item.createdBy || item.CreatedBy || item.created_by || "";
      viewStudentGradeLevel.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
      viewStudentGradeLevel.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
      viewStudentGradeLevel.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
    }
  } catch (err) {
    isOpen.value = false;
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
};

// Function to close the dialog and reset the view state
const close = () => {
  isOpen.value = false;
  resetView();
};
</script>