<template>
  <app-form-drawer
    v-model="isOpen"
    :title="isEditing ? 'Edit Class Room' : 'Create Class Room'"
    :saving="saving"
    :save-label="isEditing ? 'Save' : 'Create'"
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
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, watch, computed } from "vue";
import { classRoomApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppSelect from "components/common/AppSelect.vue";

// Component props received from the parent component.
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editingId: { type: [Object, String, Number], default: null },
  locationOptions: { type: Array, default: () => [] }
});

// Events emitted to the parent component.
const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const saving = ref(false);
const formRef = ref(null);

const isOpen = ref(props.modelValue);

// Check if we are in editing mode (handles both object or primitive id)
const isEditing = computed(() => {
  if (!props.editingId) return false;
  if (typeof props.editingId === 'object') {
    return !!(props.editingId.id || props.editingId.Id || props.editingId.classRoomId || props.editingId.ClassRoomId);
  }
  return true;
});

// Watcher to populate form when opened
watch(() => props.modelValue, (val) => {
  isOpen.value = val;
  if (val) {
    if (props.editingId && typeof props.editingId === 'object') {
      const row = props.editingId;
      // Robust locationId extraction covering various object casing & nested objects
      form.locationId = row.locationId || row.LocationId || row.location_id || row.location?.id || row.Location?.Id || null;
      form.name = row.name || row.Name || row.classRoomName || row.ClassRoomName || "";
      form.active = row.active ?? row.Active ?? true;
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

// Function to reset the form and close the drawer
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

  try {
    // Prepare the payload for API submission
    const payload = {
      locationId: form.locationId,
      name: form.name.trim(),
      active: form.active
    };

    // Extract row ID safely whether editingId is an object or primitive
    let rowId = null;
    if (props.editingId) {
      if (typeof props.editingId === 'object') {
        rowId = props.editingId.id || props.editingId.Id || props.editingId.classRoomId || props.editingId.ClassRoomId;
      } else {
        rowId = props.editingId;
      }
    }

    // Call the appropriate API method based on whether we are editing or creating a new class room
    if (rowId) {
      await classRoomApi.update(rowId, payload);
      notify.success("Class room updated successfully.");
    } else {
      await classRoomApi.create(payload);
      notify.success("Class room created successfully.");
    }

    // Clear the draft if provided, close the drawer, reset form values, and emit the saved event
    clearDraft?.();
    
    // Close the drawer and reset form values after successful submission
    isOpen.value = false;
    resetFormValues();
    emit("saved");
  } catch (err) {

    // Handle API errors, specifically checking for duplicate name errors and setting form error states accordingly
    const errCode = getApiErrorCode(err);
    const errMsg = getApiErrorMessage(err)?.toLowerCase() || '';

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