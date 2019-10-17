#!/usr/bin/env python3

import csv
import glob

# Risklevel Table:
# RISKLEVEL[Likelihood][Impact]
RISKLEVEL = {
        "H": {"L": "Low", "M": "Medium", "H": "High"},
        "M": {"L": "Low", "M": "Medium", "H": "Medium"},
        "L": {"L": "Low", "M": "Low",    "H": "Low"}
}

RES_FOLDER = "./risk_eval"


def get_risklevel(likelihood, impact):
    L = likelihood[0].upper()
    I = impact[0].upper()
    return RISKLEVEL[L][I]


def get_asset_evaluations(folder):
    evaluation_filenames = glob.glob(f"{folder}/*.csv")
    evaluation_filenames.sort()
    return evaluation_filenames


def filename_to_assetname(filename):
    return filename.split('_')[-1][:-4]


def generate_table(asset_name, filename, no_start=1):
    out = """\\subsubsection{{\\it Evaluation Asset %s }}
\\label{subsubsec:eval:%s}
\\begin{footnotesize}
\\begin{prettytablex}{lp{3cm}p{4.5cm}lll}
No. & Threat & Countermeasure(s) & L & I & Risk \\\\
\\hline
""" % (asset_name, asset_name)

    no = no_start
    with open(filename, newline='') as csvfile:
        reader = csv.DictReader(csvfile, delimiter='\t')
        for row in reader:
            risk = get_risklevel(row["Likelihood"], row["Impact"])
            out += f"{no} & {row['Threat']} & {row['Countermeasure']} "
            out += f"& {{\\it {row['Likelihood']}}} & {{\it {row['Impact']}}} & {{\it {risk}}} \\\\\n\\hline"
            no += 1

    out += """\
\\end{prettytablex}
\\end{footnotesize}

"""
    return out, no


def generate_all_tables(filenames):
    no = 1
    out = ""
    for fn in filenames:
        asset_name = filename_to_assetname(fn)
        txt, no = generate_table(asset_name, fn, no)
        out += txt
    return out

def main():
    filenames = get_asset_evaluations(RES_FOLDER)
    txt = generate_all_tables(filenames)
    print(txt)


if __name__ == '__main__':
    main()
