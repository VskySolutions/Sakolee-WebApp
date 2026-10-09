<template>
  <!-- Dialog used for creating and editing a Class Category -->
  <app-form-dialog
    v-model="formOpen"
    :title="editing ? 'Edit Class Category' : 'Create Class Category'"
    :saving="saving"
    :save-label="editing ? 'Save' : 'Create'"
    size="sm"
    @submit="submit"
    @cancel="reset"
  >
    <q-form ref="formRef" greedy>
      <!-- Class Category name -->
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
      <!-- Category Type -->
      <app-select
        v-model="form.categoryType"
        label="Category Type"
        required
        :options="categoryTypeOptions"
        option-label="label"
        option-value="value"
        emit-value
        map-options
        class="q-mb-md"
        :rules="[
          (v) => !!v || 'Category type is required'
        ]"
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
import {
  classCategoryApi,
  getApiErrorMessage
} from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppSelect from "components/common/AppSelect.vue";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editing: { type: Boolean, default: false },
  category: { type: Object, default: null }
});

const emit = defineEmits(["update:modelValue", "saved"]);
const notify = useNotify();
const formOpen = ref(props.modelValue);
const formRef = ref(null);
const saving = ref(false);
const nameError = ref("");
const form = reactive({ name: "", categoryType: "", active: true });
const categoryTypeOptions = [
  {
    label: "Category 1",
    value: "Category 1"
  },
  {
    label: "Category 2",
    value: "Category 2"
  },
  {
    label: "Category 3",
    value: "Category 3"
  }
];
// Watch for changes in the modelValue prop to open/close the form dialog
watch(
  () => props.modelValue,
  (value) => {
    formOpen.value = value;
    if (value) {
      loadForm();
    }
  }
);
// Watch for changes in the formOpen ref to emit updates to the parent component
watch(formOpen, (value) => {
  emit("update:modelValue", value);
});
// Load the form with existing category data when editing
const loadForm = () => {
  form.name = props.category?.name || "";
  form.categoryType = props.category?.categoryType || "";
  form.active = props.category?.active ?? true;
  nameError.value = "";
};
// Clear the name error when the user starts typing
const clearNameError = () => {
  if (nameError.value) {
    nameError.value = "";
  }
};
// Reset the form to its initial state
const reset = () => {
  form.name = "";
  form.categoryType = "";
  form.active = true;
  nameError.value = "";
  formRef.value?.resetValidation();
};
// Submit the form to create or update a class category
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
      categoryType: form.categoryType,
      active: form.active
    };
    if (
      props.editing && props.category?.classCategoryId
    ) {
      await classCategoryApi.update(props.category.classCategoryId, payload);
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
    // if (error?.response?.status === 409) {
    //   nameError.value = "A class category with this name already exists.";
    //   return;
    // }
    if (error?.response?.status === 409) {
      nameError.value =
        error.response.data?.message ||
        `"${form.name.trim()}" already exists in ${form.categoryType}.`;
      return;
    }
    const message = getApiErrorMessage(error, "Unable to save class category.");
    notify.error(message);
  } finally {
    saving.value = false;
  }
};
</script>
