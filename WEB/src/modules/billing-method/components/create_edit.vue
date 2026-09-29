<template>
  <app-form-drawer
    v-model="isOpen"
    :title="editingId ? 'Edit Billing Method' : 'Create Billing Method'"
    :saving="saving"
    :save-label="editingId ? 'Save' : 'Create'"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-form ref="formRef" greedy>
      <app-text-field
        v-model="form.name"
        label="Billing Method Name"
        required
        class="q-mb-md"
        :error="formErrors.name.hasError"
        :error-message="formErrors.name.message"
        @update:model-value="formErrors.name.hasError = false"
        :rules="[
          (v) => !!v?.trim() || 'Billing method name is required',
          (v) => !v || v.trim().length <= 100 || 'Billing method name cannot exceed 100 characters'
        ]"
      />

      <q-toggle v-model="form.active" label="Active" />
    </q-form>
  </app-form-drawer>
</template>

<script setup>
// Import necessary modules and components
import { ref, reactive, computed, watch } from "vue";
import { billingMethodApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppTextField from "components/common/AppTextField.vue";

// Props and Emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editingId: { type: [String, Number], default: null },
  initialData: { type: Object, default: null }
});

// Emit events to parent component
const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const saving = ref(false);
const formRef = ref(null);

// Computed property to determine if the form is in editing mode
const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

// Reactive object to hold the form data
const form = reactive({
  name: "",
  active: true
});

// Reactive object to hold form validation errors
const formErrors = reactive({
  name: {
    hasError: false,
    message: ""
  }
});

// Function to reset the form fields and validation errors
const resetForm = () => {
  form.name = "";
  form.active = true;
  formErrors.name.hasError = false;
  formErrors.name.message = "";
};

// Watchers to handle prop changes and reset form when necessary
watch(
  () => props.modelValue,
  (val) => {
    if (val) {
      if (props.editingId && props.initialData) {
        form.name = props.initialData.name || props.initialData.Name || props.initialData.billingMethodName || props.initialData.BillingMethodName || "";
        form.active = props.initialData.active ?? props.initialData.Active ?? props.initialData.is_active ?? true;
      } else {
        resetForm();
      }
    }
  }
);

// Function to handle form submission, including validation and API calls for creating or updating billing methods
const submitForm = async ({ clearDraft } = {}) => {
  formErrors.name.hasError = false;
  formErrors.name.message = "";

  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  
  try {
    // Prepare the payload for the API request, trimming whitespace from the name field
    const payload = {
      name: form.name.trim(),
      active: form.active
    };

    // Determine whether to create a new billing method or update an existing one based on the presence of editingId
    if (props.editingId) {
      await billingMethodApi.update(props.editingId, payload);
      notify.success("Billing method updated successfully.");
    } else {
      await billingMethodApi.create(payload);
      notify.success("Billing method created successfully.");
    }

    // Drop the draft if the clearDraft function is provided, emit a saved event, and reset the form
    clearDraft?.();
    emit("saved");
    isOpen.value = false;
    resetForm();
  } catch (err) {
    // Handle specific API error codes, such as duplicate identifiers, and display appropriate error messages
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      formErrors.name.hasError = true;
      formErrors.name.message = "A billing method with this name already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>