<template>
    <!-- Drawer used for creating and editing a Billing Cycle -->
  <app-form-drawer
    v-model="formOpen"
    :title="editing ? 'Edit Billing Cycle' : 'Create Billing Cycle'"
    :saving="saving"
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
    </q-form>
  </app-form-drawer>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { billingCycleApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppTextField from "components/common/AppTextField.vue";
// Define the properties received from the parent component.
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  // Indicates whether the form is used for editing.
  editing: {
    type: Boolean,
    default: false
  },
  // Contains the Billing Cycle data when editing.
  billingCycle: {
    type: Object,
    default: null
  }
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

// Form data.
const form = reactive({
  name: ""
});

// Watch for changes to the drawer state from the parent.
watch(
  () => props.modelValue,
  (value) => {
    formOpen.value = value;
    // Load form data when the drawer is opened.
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
  nameError.value = "";
  formRef.value?.resetValidation();
};

// Validate and save the Billing Cycle.
const submit = async ({ clearDraft } = {}) => {
  nameError.value = "";
  const valid = await formRef.value?.validate();
  if (!valid) {
    return;
  }
  saving.value = true;
  try {
    const payload = {
      name: form.name.trim()
    };
    // Update the existing Billing Cycle when editing.
    if (props.editing && props.billingCycle?.billingCycleId) {
      await billingCycleApi.update(
        props.billingCycle.billingCycleId,
        payload
      );
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
    if (error?.response?.status === 409) {
      nameError.value ="A billing cycle with this name already exists.";
      return;
    }
    // Get a user-friendly error message from the API.
    const message = getApiErrorMessage(error,"Unable to save billing cycle.");
    notify.error(message);
  } finally {
    saving.value = false;
  }
};
</script>
