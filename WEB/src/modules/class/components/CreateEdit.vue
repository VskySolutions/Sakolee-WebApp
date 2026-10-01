<template>
  <q-page padding>
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Classes', to: { name: 'classes' } },
        { label: isEdit ? 'Edit Class' : 'Add Class' }
      ]"
      :back-to="{ name: 'classes' }"
    />

    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <q-card v-else flat bordered class="q-pa-md">
      <q-form ref="formRef" greedy @submit.prevent="submitForm">
        <class-form-fields v-model="form" :show-active-toggle="isEdit" />

        <div class="row justify-end q-gutter-sm q-mt-md">
          <q-btn flat no-caps label="Cancel" color="grey" :to="{ name: 'classes' }" />
          <q-btn unelevated no-caps color="primary" :label="isEdit ? 'Update' : 'Save'" :loading="saving" @click="submitForm" />
        </div>
      </q-form>
    </q-card>
  </q-page>
</template>

<script setup>
import { onMounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { classApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { blankClassForm, classFormFromRow, toClassPayload } from "composables/classForm";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
import ClassFormFields from "components/class/ClassFormFields.vue";

const route = useRoute();
const router = useRouter();
const notify = useNotify();

// Shared by the class_create ("/classes/create") and class_edit ("/classes/:id/edit") routes —
// edit mode whenever the route carries an id.
const classId = route.params.id;
const isEdit = !!classId;
const loading = ref(false);
const saving = ref(false);
const formRef = ref(null);
const form = reactive(blankClassForm());

onMounted(async () => {
  if (!isEdit) return;
  loading.value = true;
  try {
    const row = await classApi.get(classId);
    Object.assign(form, classFormFromRow(row));
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
});

const submitForm = async () => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    if (isEdit) {
      await classApi.update(classId, { ...toClassPayload(form), active: form.active });
      notify.success("Class updated.");
    } else {
      await classApi.create(toClassPayload(form));
      notify.success("Class created.");
    }
    router.push({ name: "classes" });
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};
</script>
