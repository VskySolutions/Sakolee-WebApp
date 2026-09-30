<template>
  <q-page padding>
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Classes', to: { name: 'classes' } },
        { label: 'Add Class' }
      ]"
      :back-to="{ name: 'classes' }"
    />

    <q-card flat bordered class="q-pa-md">
      <q-form ref="formRef" greedy @submit.prevent="submitForm">
        <class-form-fields v-model="form" />

        <div class="row justify-end q-gutter-sm q-mt-md">
          <q-btn flat no-caps label="Cancel" color="grey" :to="{ name: 'classes' }" />
          <q-btn unelevated no-caps color="primary" label="Save" :loading="saving" @click="submitForm" />
        </div>
      </q-form>
    </q-card>
  </q-page>
</template>

<script setup>
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { classApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { blankClassForm, toClassPayload } from "composables/classForm";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
import ClassFormFields from "components/class/ClassFormFields.vue";

const router = useRouter();
const notify = useNotify();

const formRef = ref(null);
const saving = ref(false);
const form = reactive(blankClassForm());

const submitForm = async () => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    await classApi.create(toClassPayload(form));
    notify.success("Class created.");
    router.push({ name: "classes" });
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};
</script>
