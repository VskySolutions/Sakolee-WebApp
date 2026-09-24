<template>
   <!-- Drawer used for creating or editing a Class Category -->
  <app-form-drawer
    v-model="formOpen"
    :title="editing ? 'Edit Class Category' : 'Create Class Category'"
    :saving="saving"
    @submit="submit"
    @cancel="reset"
  >
  <!-- Form container with validation -->
    <q-form ref="formRef" greedy>
      <!-- Class Category name field -->
      <app-text-field
        v-model="form.name"
        label="Name"
        required
        class="q-mb-md"
        :error="!!nameError"
        :error-message="nameError"
        :rules="[
          (v) => !!v?.trim() || 'Category name is required'
        ]"
        @update:model-value="clearNameError"
      />
      <!-- Class Category type dropdown -->
      <app-select
        v-model="form.categoryType"
        label="Category Type"
        required
        :options="categoryTypeOptions"
        option-label="label"
        option-value="value"
        emit-value
        map-options
        :rules="[
          (v) => !!v || 'Category type is required'
        ]"
      />
    </q-form>
  </app-form-drawer>
</template>
<script setup>
import { reactive, ref, watch } from "vue";
import { classCategoryApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppSelect from "components/common/AppSelect.vue";
// Component props received from the parent component.
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editing: { type: Boolean, default: false },
  category: { type: Object, default: null }
});
// Events emitted to the parent component.
const emit = defineEmits([ "update:modelValue", "saved" ]);
// Notification helper used to show success and error messages.
const notify = useNotify();
const formOpen = ref(props.modelValue);
const formRef = ref(null);
const saving = ref(false);
// Stores duplicate name validation errors.
const nameError = ref("");
// Form data for Class Category.
const form = reactive({ name: "", categoryType: "" });
// Available options for the Category Type dropdown.
const categoryTypeOptions = [
  { label: "Category 1", value: "Category 1" },
  { label: "Category 2", value: "Category 2" },
  { label: "Category 3", value: "Category 3" }
];

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

// Emit the drawer state changes back to the parent.
watch(formOpen, (value) => {
  emit("update:modelValue", value);
});

// Load existing category data when editing.
// Reset the fields when creating a new category.
const loadForm = () => {
  form.name = props.category?.name || "";
  form.categoryType = props.category?.categoryType || "";
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
  form.categoryType = "";
  nameError.value = "";
  formRef.value?.resetValidation();
};

// Validate and save the Class Category.
const submit = async ({ clearDraft } = {}) => {
  nameError.value = "";
   // Validate all form fields before submitting.
  const valid = await formRef.value?.validate();
  if (!valid) {
    return;
  }
  saving.value = true;
  try {
    const payload = {
      name: form.name.trim(), categoryType: form.categoryType
    };
    if (props.editing && props.category?.classCategoryId) {
      await classCategoryApi.update( props.category.classCategoryId, payload );
      notify.success("Class category updated.");
    } else {
      await classCategoryApi.create(payload);
      notify.success("Class category created.");
    }
    clearDraft?.();
    formOpen.value = false;
    reset();
    emit("saved");
 } catch (error) {
    if (error?.response?.status === 409) {
      nameError.value ="A class category with this name already exists.";
      return;
    }
    const message = getApiErrorMessage(error, "Unable to save class category.");
    notify.error(message);
  } finally {
    saving.value = false;
  }
};
</script>
