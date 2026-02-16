<script setup lang="ts">
import { CurrencyInputPart, CurrencyPartType, stringToPartType as keyStringToPartType, partsToValue } from './currency-input-part';

const props = withDefaults(defineProps<{readonly?: boolean}>(), { readonly: false });
const model = defineModel<number>();
const parts = ref<CurrencyInputPart[]>([new CurrencyInputPart(CurrencyPartType.Plus, model.value!)]);

const formattedValue = computed<string>(() => {
  if (!model.value) return '0.00';
  let fmt = formattedPart(parts.value[0]!.amount);
  for(let i = 1; i < parts.value.length; i += 1) {
    fmt += (parts.value[i]!.partType == CurrencyPartType.Plus) ? " + " : " - ";
    fmt += formattedPart(parts.value[i]!.amount);
  }
  return fmt;
});

function formattedPart(value: number | undefined): string {
  value ??= 0;
  let strValue = value.toString();
  strValue = strValue.padStart(3, '0');
  return strValue.slice(0, strValue.length - 2) + '.' + strValue.slice(strValue.length - 2);
}

function handleInput(keyboardEvent: KeyboardEvent) {
  if (!model.value) parts.value = [new CurrencyInputPart(CurrencyPartType.Plus, 0)];
  if (keyboardEvent.key != "Tab") 
    keyboardEvent.preventDefault();

  if (props.readonly) return;

  const modelValue = parts.value.at(-1)!;
  // console.log(keyboardEvent.key);
  if (keyboardEvent.key == "Backspace") {
    if(modelValue.amount == 0) {
      if(parts.value.length > 1) 
        parts.value.pop();
    } else {
      modelValue.amount -= modelValue.amount % 10;
      modelValue.amount /= 10;
    }
  } else if(
    keyboardEvent.key == "+" || keyboardEvent.key == "-" ||
    keyboardEvent.key == "Add" || keyboardEvent.key == "Subtract"
  ) {
    const partType = keyStringToPartType(keyboardEvent.key);
    if(modelValue.amount == 0) {
      modelValue.partType = partType;
    } else {
      parts.value.push(new CurrencyInputPart(partType, 0));
    }
  } else {
    const number = parseInt(keyboardEvent.key);
    if (!Number.isNaN(number)) {
        modelValue.amount *= 10;
        modelValue.amount += number;
    }
  }
  model.value = partsToValue(parts.value);
}
</script>

<template>
  <input :value="formattedValue" type="tel" :disabled="props.readonly"
    class="border rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500" @keydown="handleInput"
  /> 
</template>

<style scoped>
input {
  /* Make the input look like a regular text field but not editable */
  cursor: default;
}

input:focus {
  border-color: #3b82f6;
  /* blue-500 */
}

input::-webkit-outer-spin-button,
input::-webkit-inner-spin-button {
  /* display: none; <- Crashes Chrome on hover */
  -webkit-appearance: none;
  margin: 0;
  /* <-- Apparently some margin are still there even though it's hidden */
}

input[type=number] {
  appearance: inherit;
  -moz-appearance: textfield;
  /* Firefox */
}
</style>