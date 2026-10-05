<template>
  <app-form-dialog
    v-model="isOpen"
    :title="isEditing ? 'Edit Class Room' : 'Create Class Room'"
    :saving="saving"
    :save-label="isEditing ? 'Save' : 'Create'"
    size="sm"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-form ref="formRef" greedy>
      <!-- Location Selection Field using app-select -->
      <app-select
        v-model="form.locationId"
        label="Location"
        required
        :options="locationOptions"
        option-value="id"
        option-label="name"
        emit-value
        map-options
        clearable
        class="q-mb-md"
        :error="formErrors.locationId.hasError"
        :error-message="formErrors.locationId.message"
        @update:model-value="formErrors.locationId.hasError = false"
        :rules="[(v) => !!v || 'Location is required']"
      />

      <!-- Class Room Name Field -->
      <app-text-field
        v-model="form.name"
        label="Class Room Name"
        required
        class="q-mb-md"
        :error="formErrors.name.hasError"
        :error-message="formErrors.name.message"
        @update:model-value="formErrors.name.hasError = false"
        :rules="[
          (v) => !!v?.trim() || 'Class room name is required',
          (v) =>
            !v ||
            v.trim().length <= 100 ||
            'Class room name cannot exceed 100 characters'
        ]"
      />

      <!-- Status Toggle -->
      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-dialog>
</template>

<script setup>
// Import necessary modules and components
import { ref, reactive, watch, computed } from "vue";
import { classRoomApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppSelect from "components/common/AppSelect.vue";

// Component props received from the parent component.
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editingId: { type: [Object, String, Number], default: null },
  initialData: { type: Object, default: null },
  locationOptions: { type: Array, default: () => [] }
});

// Events emitted to the parent component.
const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const saving = ref(false);
const formRef = ref(null);

const isOpen = ref(props.modelValue);

// Check if we are in editing mode
const isEditing = computed(() => !!props.editingId);

// Watcher to populate form when opened
watch(() => props.modelValue, (val) => {
  isOpen.value = val;
  if (val) {
    if (props.editingId && props.initialData) {
      const row = props.initialData;
      form.locationId = row.locationId || row.LocationId || row.location_id || row.location?.id || row.Location?.Id || null;
      form.name = row.name || row.Name || row.classRoomName || row.ClassRoomName || "";
      form.active = row.active ?? row.Active ?? row.is_active ?? true;
    } else {
      resetFormValues();
    }
  }
});

// Watcher to emit changes to the parent component
watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

// Reactive form data and error states
const form = reactive({
  locationId: null,
  name: "",
  active: true
});

// Reactive form error states
const formErrors = reactive({
  locationId: { hasError: false, message: "" },
  name: { hasError: false, message: "" }
});

// Function to reset form values to their default state
const resetFormValues = () => {
  form.locationId = null;
  form.name = "";
  form.active = true;
  formErrors.locationId.hasError = false;
  formErrors.locationId.message = "";
  formErrors.name.hasError = false;
  formErrors.name.message = "";
};

// Function to reset the form and close the dialog
const resetForm = () => {
  resetFormValues();
  isOpen.value = false;
};

// Function to submit the form data to the API, handling both create and update operations
const submitForm = async ({ clearDraft } = {}) => {
  formErrors.locationId.hasError = false;
  formErrors.locationId.message = "";
  formErrors.name.hasError = false;
  formErrors.name.message = "";

  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  // Prepare the payload and call the appropriate API method based on whether we are editing or creating a new class room
  try {
    // Prepare the payload for the API request
    const payload = {
      locationId: form.locationId,
      name: form.name.trim(),
      active: form.active
    };

    // Call the appropriate API method based on whether we are editing or creating a new class room
    if (props.editingId) {
      await classRoomApi.update(props.editingId, payload);
      notify.success("Class room updated successfully.");
    } else {
      await classRoomApi.create(payload);
      notify.success("Class room created successfully.");
    }

    // Clear any draft data if applicable, close the dialog, reset the form, and emit a saved event
    clearDraft?.();
    isOpen.value = false;
    resetFormValues();
    emit("saved");
  } catch (err) {
    const errCode = getApiErrorCode(err);
    const errMsg = getApiErrorMessage(err)?.toLowerCase() || '';

    // Handle specific API error codes for duplicate identifiers and provide user-friendly error messages
    if (errCode === ApiErrorCodes.DuplicateIdentifier || errMsg.includes('already exists')) {
      formErrors.name.hasError = true;
      formErrors.name.message = "A class room with this name already exists in the selected location.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>