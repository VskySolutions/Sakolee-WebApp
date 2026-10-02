import { ref, computed, onMounted } from "vue";
import { getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

// A simple name-only master (T-Shirt Size, Hear About Us, …) as { label, value } select options: the
// tenant's active records, in the order they were added to the master. The record it feeds stores the
// choice as text (e.g. Student.TShirtSize, Family.Source), so the option value is the master's name —
// values saved before the master existed keep matching.
//
// `getCurrentValues` (optional) returns the value(s) already saved on the record(s) being edited: one
// no longer in the active master (deactivated, renamed, or entered before the master existed) is still
// offered, so opening and saving a record never silently clears it.
export function useMasterNameOptions (listApi, getCurrentValues = () => []) {
  const notify = useNotify();
  const items = ref([]);
  const loading = ref(false);

  const options = computed(() => {
    const list = items.value.map((i) => ({ label: i.name, value: i.name }));
    const current = [].concat(getCurrentValues() || []).filter(Boolean);
    for (const value of current) {
      if (!list.some((o) => o.value === value)) {
        list.unshift({ label: value, value });
      }
    }
    return list;
  });

  const load = async () => {
    loading.value = true;
    try {
      const res = await listApi({ active: true, limit: 100, sortBy: "createdOnUtc", descending: false });
      items.value = res?.data || [];
    } catch (err) {
      notify.error(getApiErrorMessage(err));
    } finally {
      loading.value = false;
    }
  };

  onMounted(load);

  return { options, loading, reload: load };
}
