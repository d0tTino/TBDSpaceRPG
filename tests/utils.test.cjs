const assert = require('assert');
const { parseJson } = require('../servers/utils.cjs');

assert.strictEqual(parseJson(''), null);
assert.strictEqual(parseJson('   '), null);
assert.strictEqual(parseJson('{'), null);
assert.deepStrictEqual(parseJson('{"x":1}'), { x: 1 });

console.log('Utils tests passed');
