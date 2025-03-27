import base64
import uuid
import random

EXTRA_CHARS = "\"%?+=<>:;?!&'()*,-./_"

def gen_hash():
	random_uuid = str(uuid.uuid4())
	b64_uuid = base64.b64encode(random_uuid.encode(encoding="utf8")).decode(encoding="utf-8")
	hash_base = b64_uuid[:18]
	hash = ""
	for i in hash_base:
		if random.random() > 0.7:
			hash += random.choice(EXTRA_CHARS)
		else:
			hash += i
	return hash

def gen_barcode():
	barcode = "952"
	for i in range(10):
		barcode += random.choice("0123456789")
	return barcode

def gen_chzn():
	return f"010{gen_barcode()}211{gen_hash()}"

def write_file(path, number):
	with open(path, "w") as f:
		for i in range(number):
			f.write(gen_chzn() + "\n")

if __name__ == "__main__":
	path = input("Enter file path: ")
	nr = int(input("How many barcodes to generate? "))
	write_file(path, nr)
	print("OK")