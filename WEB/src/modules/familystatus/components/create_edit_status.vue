<template>
  <app-form-drawer
    v-model="isOpen"
    :title="isEditing ? 'Edit Family Status' : 'Create Family Status'"
    :saving="saving"
    :save-label="isEditing ? 'Save' : 'Create'"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-form ref="formRef" greedy>
      <app-text-field
        v-model="form.familyStatusName"
        label="Family Status Name"
        required
        class="q-mb-md"
        :error="formErrors.familyStatusName.hasError"
        :error-message="formErrors.familyStatusName.message"
        @update:model-value="formErrors.familyStatusName.hasError = false"
        :rules="[
          (v) => !!v?.trim() || 'Family status name is required',
          (v) =>
            !v ||
            v.trim().length <= 100 ||
            'Family status name cannot exceed 100 characters'
        ]"
      />

      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-drawer>
</template>

<script setup>
// Import necessary modules and components
import { ref, reactive, watch, computed } from "vue";
import { familyStatusApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppTextField from "components/common/AppTextField.vue";

// Define props for the component
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editingId: { type: [Object, String, Number], default: null },
  initialData: { type: Object, default: null }
});

// Define emits for the component
const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const saving = ref(false);
const formRef = ref(null);
const isOpen = ref(props.modelValue);

// Computed property to determine if the form is in editing mode
const isEditing = computed(() => !!props.editingId);

// Define reactive state for the form and form errors
const form = reactive({
  familyStatusName: "",
  active: true
});

// Define reactive state for form errors
const formErrors = reactive({
  familyStatusName: {
    hasError: false,
    message: ""
  }
});

// Watch for changes in the modelValue prop to open or close the form drawer
watch(() => props.modelValue, (val) => {
  isOpen.value = val;
  if (val) {
    if (props.editingId && props.initialData) {
      form.familyStatusName = props.initialData.familyStatusName || props.initialData.FamilyStatusName || props.initialData.name || props.initialData.Name || "";
      form.active = props.initialData.active ?? props.initialData.Active ?? props.initialData.is_active ?? true;
    } else {
      resetFormValues();
    }
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

// Function to reset form values to their default state
const resetFormValues = () => {
  form.familyStatusName = "";
  form.active = true;
  formErrors.familyStatusName.hasError = false;
  formErrors.familyStatusName.message = "";
};

// Function to reset the form and close the drawer
const resetForm = () => {
  resetFormValues();
  isOpen.value = false;
};

// Function to handle form submission for creating or updating a family status
const submitForm = async ({ clearDraft } = {}) => {
  formErrors.familyStatusName.hasError = false;
  formErrors.familyStatusName.message = "";

  // Validate the form before proceeding
  if (!(await formRef.value?.validate())) {
    return;
  }

  // Set the saving state to true while the API request is in progress
  saving.value = true;

  try {
    // Prepare the payload for the API request
    const payload = {
      name: form.familyStatusName.trim(),
      familyStatusName: form.familyStatusName.trim(),
      active: form.active
    };

    // Determine whether to create a new record or update an existing one based on the editingId prop
    if (props.editingId) {
      await familyStatusApi.update(props.editingId, payload);
      notify.success("Family status updated successfully.");
    } else {
      await familyStatusApi.create(payload);
      notify.success("Family status created successfully.");
    }

    // Clear the draft, close the form drawer, reset form values, and emit the saved event
    clearDraft?.();
    isOpen.value = false;
    resetFormValues();
    emit("saved");
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier || getApiErrorMessage(err)?.toLowerCase().includes('already exists')) {
      formErrors.familyStatusName.hasError = true;
      formErrors.familyStatusName.message = "A family status with this name already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>