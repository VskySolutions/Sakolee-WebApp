<template>
  <app-form-drawer
    v-model="isOpen"
    :title="isEditing ? 'Edit Family Relation' : 'Create Family Relation'"
    :saving="saving"
    :save-label="isEditing ? 'Save' : 'Create'"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-form ref="formRef" greedy>
      <app-text-field
        v-model="form.name"
        label="Relation Name"
        required
        class="q-mb-md"
        :error="formErrors.name.hasError"
        :error-message="formErrors.name.message"
        @update:model-value="formErrors.name.hasError = false"
        :rules="[
          (v) => !!v?.trim() || 'Relation name is required',
          (v) => !v || v.trim().length <= 100 || 'Name cannot exceed 100 characters'
        ]"
      />
      <q-toggle v-model="form.active" label="Active" />
    </q-form>
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { familyRelationApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppTextField from "components/common/AppTextField.vue";

// Props and Emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editingId: { type: [String, Number], default: null },
  initialData: { type: Object, default: null }
});

const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const isOpen = ref(props.modelValue);
const saving = ref(false);
const formRef = ref(null);

const isEditing = computed(() => !!props.editingId);

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

// Watchers to handle prop changes and reset form when necessary
watch(() => props.modelValue, (val) => {
  isOpen.value = val;
  if (val) {
    if (props.editingId && props.initialData) {
      form.name = props.initialData.name || "";
      form.active = props.initialData.active ?? true;
    } else {
      resetFormValues();
    }
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

// Function to reset the form values to their initial state
const resetFormValues = () => {
  form.name = "";
  form.active = true;
  formErrors.name.hasError = false;
  formErrors.name.message = "";
};

// Function to reset the form and close the drawer
const resetForm = () => {
  resetFormValues();
  isOpen.value = false;
};

// Function to handle form submission for creating or updating a family relation
const submitForm = async ({ clearDraft } = {}) => {
  formErrors.name.hasError = false;
  formErrors.name.message = "";

  if (!(await formRef.value?.validate())) {
    return;
  }

  // Set saving state to true to indicate that the form submission is in progress
  saving.value = true;

  try {
    // Prepare the payload for the API request
    const payload = {
      name: form.name.trim(),
      active: form.active
    };

    // Call the appropriate API method based on whether we are editing or creating a new relation
    if (isEditing.value) {
      await familyRelationApi.update(props.editingId, payload);
      notify.success("Family relation updated successfully.");
    } else {
      await familyRelationApi.create(payload);
      notify.success("Family relation created successfully.");
    }

    // Clear the draft if applicable, close the drawer, reset the form values, and emit the "saved" event
    clearDraft?.();
    isOpen.value = false;
    resetFormValues();
    emit("saved");
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      formErrors.name.hasError = true;
      formErrors.name.message = "A relation with this name already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>