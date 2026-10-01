<template>
<<<<<<< HEAD
  <!-- Dialog used for creating and editing a Billing Cycle -->
  <app-form-dialog
=======
  <!-- Drawer used for creating and editing a Billing Cycle -->
  <app-form-drawer
>>>>>>> 5dfdb44bfd54d7af21c852b84d64ddd32f8a6cd6
    v-model="formOpen"
    :title="editing ? 'Edit Billing Cycle' : 'Create Billing Cycle'"
    :saving="saving"
    :save-label="editing ? 'Save' : 'Create'"
    size="sm"
    @submit="submit"
    @cancel="reset"
  >
    <!-- Form container with validation -->
    <q-form ref="formRef" greedy>
      <!-- Billing Cycle name field -->
      <app-text-field
        v-model="form.name"
        label="Name"
        required
        class="q-mb-md"
        :error="!!nameError"
        :error-message="nameError"
        :rules="[
          (v) => !!v?.trim() || 'Billing cycle name is required'
        ]"
        @update:model-value="clearNameError"
      />
      <!-- Active status -->
      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-dialog>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { billingCycleApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";
// Define the properties received from the parent component.
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editing: { type: Boolean, default: false },
  billingCycle: { type: Object, default: null }
});
// Events sent back to the parent component.
const emit = defineEmits([
  "update:modelValue",
  "saved"
]);

const notify = useNotify();
const formOpen = ref(props.modelValue);
const formRef = ref(null);
const saving = ref(false);
const nameError = ref("");
const form = reactive({ name: "", active: true });
// Watch for changes to the drawer state from the parent.
watch(
  () => props.modelValue,
  (value) => {
    formOpen.value = value;
    if (value) {
      loadForm();
    }
  }
);
// Send drawer state changes back to the parent.
watch(formOpen, (value) => {
  emit("update:modelValue", value);
});
// Load existing Billing Cycle data when editing.
const loadForm = () => {
  form.name = props.billingCycle?.name || "";
  form.active = props.billingCycle?.active ?? true;
  nameError.value = "";
};
// Clear the duplicate name error when the user changes the name.
const clearNameError = () => {
  if (nameError.value) {
    nameError.value = "";
  }
};
// Reset the form to its initial state.
const reset = () => {
  form.name = "";
  form.active = true;
  nameError.value = "";
  formRef.value?.resetValidation();
};
// Handle form submission for creating or updating a Billing Cycle.
const submit = async ({ clearDraft } = {}) => {
  nameError.value = "";
  // Validate the form before submission.
  const valid = await formRef.value?.validate();
  if (!valid) {
    return;
  }
  saving.value = true;
  // Prepare the payload for the API request.
  try {
    const payload = {
      name: form.name.trim(),
      active: form.active
    };
    // Update the existing Billing Cycle record when editing.
    if (props.editing && props.billingCycle?.billingCycleId) {
      await billingCycleApi.update(props.billingCycle.billingCycleId, payload);
      notify.success("Billing cycle updated.");
    } else {
      await billingCycleApi.create(payload);
      notify.success("Billing cycle created.");
    }
    clearDraft?.();
    formOpen.value = false;
    reset();
    emit("saved");
  } catch (error) {
    // Handle duplicate-name validation from the API.
    if (error?.response?.status === 409) {
      nameError.value = "A billing cycle with this name already exists.";
      return;
    }
    // Handle other API errors and notify the user.
    const message = getApiErrorMessage(error, "Unable to save billing cycle.");
    notify.error(message);
  } finally {
    saving.value = false;
  }
};
</script>
