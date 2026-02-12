<script setup lang="ts">
const model = defineModel<number>();

const formattedValue = computed<string>(() => {
  if (!model.value) return '0.00';

  let strValue = model.value.toString();
  strValue = strValue.padStart(3, '0');
  return strValue.slice(0, strValue.length - 2) + '.' + strValue.slice(strValue.length - 2);
});

function handleInput(keyboardEvent: KeyboardEvent) {
  if (keyboardEvent.key != "Tab")
    keyboardEvent.preventDefault();

  if (keyboardEvent.key == "Backspace") {
    if (model.value) {
      let modelValue = model.value;
      modelValue -= modelValue % 10;
      modelValue /= 10;
      model.value = modelValue;
    }
  } else {
    const number = parseInt(keyboardEvent.key);
    if (!Number.isNaN(number)) {
      if (!model.value) model.value = number;
      else {
        let modelValue = model.value;
        modelValue *= 10;
        modelValue += number;
        model.value = modelValue;
      }
    }
  }
}
</script>

<template>
  <input ref="inputRef" :value="formattedValue" type="tel"
    class="border rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500" @keydown="handleInput" />
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