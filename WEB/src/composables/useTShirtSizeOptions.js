import { tShirtSizeApi } from "services/api";
import { useMasterNameOptions } from "composables/useMasterNameOptions";

// T-Shirt Size master as select options for a student's size (Student.TShirtSize is text). See
// useMasterNameOptions for ordering and how an already-saved size outside the master is kept.
export function useTShirtSizeOptions (getCurrentValues) {
  return useMasterNameOptions(tShirtSizeApi.list, getCurrentValues);
}
