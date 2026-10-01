<template>
  <!-- Dialog used for creating and editing an E-Payment Schedule record -->
  <app-form-dialog
    v-model="formOpen"
    :title="editing ? 'Edit E-Payment Schedule' : 'Create E-Payment Schedule'"
    :saving="saving"
    :save-label="editing ? 'Save' : 'Create'"
    size="sm"
    @submit="submit"
    @cancel="reset"
  >
    <!-- Form container with validation -->
    <q-form ref="formRef" greedy>
      <!-- E-Payment Schedule name field -->
      <app-text-field
        v-model="form.name"
        label="Name"
        required
        class="q-mb-md"
        :error="!!nameError"
        :error-message="nameError"
        :rules="[
          (v) => !!v?.trim() || 'E-Payment Schedule name is required'
        ]"
        @update:model-value="clearNameError"
      />
      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-dialog>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { ePaymentScheduleApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";

// Define the properties received from the parent component.
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  // Indicates whether the form is used for editing.
  editing: { type: Boolean, default: false },
  // Contains the E-Payment Schedule data when editing.
  ePaymentSchedule: { type: Object, default: null }
});

// Events sent back to the parent component.
const emit = defineEmits(["update:modelValue", "saved"]);
const notify = useNotify();
const formOpen = ref(props.modelValue);
const formRef = ref(null);
const saving = ref(false);
const nameError = ref("");
// Form data.
const form = reactive({ name: "", active: true });
// Watch for changes to the dialog state from the parent.
watch(
  () => props.modelValue, (value) => {
    formOpen.value = value;
    // Load form data when the dialog is opened.
    if (value) {
      loadForm();
    }
  }
);
// Send dialog state changes back to the parent.
watch(formOpen, (value) => {
  emit("update:modelValue", value);
});
// Load existing E-Payment Schedule data when editing.
const loadForm = () => {
  form.name = props.ePaymentSchedule?.name || "";
  form.active = props.ePaymentSchedule?.active ?? true;
  nameError.value = "";
};
// Clear the duplicate name error when the user changes the name.
const clearNameError = () => {
  if (nameError.value) {
    nameError.value = "";
  }
};
// Reset the form fields and validation.
const reset = () => {
  form.name = "";
  form.active = true;
  nameError.value = "";
  formRef.value?.resetValidation();
};
// Validate and save the E-Payment Schedule record.
const submit = async ({ clearDraft } = {}) => {
  nameError.value = "";
  const valid = await formRef.value?.validate();
  if (!valid) {
    return;
  }
  saving.value = true;
  try {
    const payload = {
      name: form.name.trim(),
      active: form.active
    };
    // Update the existing E-Payment Schedule record when editing.
    if (
      props.editing && props.ePaymentSchedule?.ePaymentScheduleId
    ) {
      await ePaymentScheduleApi.update(props.ePaymentSchedule.ePaymentScheduleId, payload);
      notify.success("E-Payment Schedule updated.");
    } else {
      await ePaymentScheduleApi.create(payload);
      notify.success("E-Payment Schedule created.");
    }
    clearDraft?.();
    formOpen.value = false;
    reset();
    emit("saved");
  } catch (error) {
    // Handle duplicate-name validation from the API.
    if (error?.response?.status === 409) {
      nameError.value = "An E-Payment Schedule record with this name already exists.";
      return;
    }
    // Get a user-friendly error message from the API.
    const message = getApiErrorMessage(error, "Unable to save E-Payment Schedule.");
    notify.error(message);
  } finally {
    saving.value = false;
  }
};
</script>
