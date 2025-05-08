<template>
    <div v-if="appearanceMethod?.length">
        <span v-for="method in appearanceMethod">
        <div>
            <span>
            {{
                `${method.roleTypeDesc} appearing by ${method.appearanceMethodDesc}`
            }}
            </span>
        </div>
        </span>
    </div>
</template>

<script setup lang="ts">
  defineProps<{
    appearanceMethod: any[];
  }>();
</script>
