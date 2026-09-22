<template>
  <app-form-drawer
    v-model="formOpen"
    :title="editing ? 'Edit Class Category' : 'Create Class Category'"
    :saving="saving"
    @submit="submit"
    @cancel="reset"
  >
    <q-form ref="formRef" greedy>
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

import {
  classCategoryApi,
  getApiErrorMessage
} from "services/api";

import { useNotify } from "composables/useNotify";

import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppSelect from "components/common/AppSelect.vue";

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  editing: {
    type: Boolean,
    default: false
  },
  category: {
    type: Object,
    default: null
  }
});

const emit = defineEmits([
  "update:modelValue",
  "saved"
]);

const notify = useNotify();

const formOpen = ref(props.modelValue);
const formRef = ref(null);
const saving = ref(false);

const nameError = ref("");

const form = reactive({
  name: "",
  categoryType: ""
});

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

watch(
  () => props.modelValue,
  (value) => {
    formOpen.value = value;

    if (value) {
      loadForm();
    }
  }
);

watch(formOpen, (value) => {
  emit("update:modelValue", value);
});

const loadForm = () => {
  form.name = props.category?.name || "";
  form.categoryType = props.category?.categoryType || "";
  nameError.value = "";
};

const clearNameError = () => {
  if (nameError.value) {
    nameError.value = "";
  }
};

const reset = () => {
  form.name = "";
  form.categoryType = "";
  nameError.value = "";
  formRef.value?.resetValidation();
};

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
      categoryType: form.categoryType
    };

    if (props.editing && props.category?.classCategoryId) {
      await classCategoryApi.update(
        props.category.classCategoryId,
        payload
      );

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
      nameError.value =
        "A class category with this name already exists.";
      return;
    }

    const message = getApiErrorMessage(
      error,
      "Unable to save class category."
    );

    notify.error(message);
  } finally {
    saving.value = false;
  }
};
</script>
