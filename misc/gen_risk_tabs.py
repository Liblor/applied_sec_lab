#!/usr/bin/env python3

import csv
import glob
import warnings

# Risklevel Table:
# RISKLEVEL[Likelihood][Impact]
RISKLEVEL = {
        "H": {"L": "Low", "M": "Medium", "H": "High"},
        "M": {"L": "Low", "M": "Medium", "H": "Medium"},
        "L": {"L": "Low", "M": "Low",    "H": "Low"}
}

RES_FOLDER = "./risk_eval"
OUT_FOLDER = "./output/"

ADDITIONAL_CNTR_MSRS_TITLE = "Additional Countermeasures"

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


def generate_table(asset_name, filename, no_start=1, risk_acceptance=""):
    out = """\\subsubsection{{\\it Evaluation Asset %s }}
\\label{subsubsec:eval:%s}
\\begin{footnotesize}
\\begin{prettytablex}{lp{2.5cm}p{3cm}lll}
No. & Threat & Countermeasure(s) & L & I & Risk \\\\
\\hline
""" % (asset_name, asset_name)

    no = no_start
    with open(filename, newline='') as csvfile:
        reader = csv.DictReader(csvfile, delimiter='\t')
        for row in reader:
            risk = get_risklevel(row["Likelihood"], row["Impact"])

            if risk != "Low":
                if ADDITIONAL_CNTR_MSRS_TITLE not in row:
                    print(row['Threat'])
                    warnings.warn(f"Missing additional countermeasures for threat {no} of risk {risk}!")
                else:
                    risk_acceptance += f"{no} & {row[ADDITIONAL_CNTR_MSRS_TITLE]}"

            out += f"{no} & {row['Threat']} & {row['Countermeasure']} "
            out += f"& {{\\it {row['Likelihood']}}} & {{\it {row['Impact']}}} & {{\it {risk}}} \\\\\n\\hline"
            no += 1

    out += """\
\\end{prettytablex}
\\end{footnotesize}

"""

    return out, risk_acceptance, no


def generate_all_tables(filenames):
    no = 1
    out = ""

    risk_acceptance = """\\begin{footnotesize}
\\begin{prettytablex}{p{2cm}X}
No. of threat & Proposed additional countermeasure including expected impact  \\\\
\\hline"""

    for fn in filenames:
        asset_name = filename_to_assetname(fn)
        txt, risk_acceptance, no = generate_table(asset_name, fn, no, risk_acceptance)
        out += txt

    risk_acceptance += """
\\end{prettytablex}
\\end{footnotesize}
"""
    return out, risk_acceptance

def main():
    filenames = get_asset_evaluations(RES_FOLDER)
    txt, risk_acceptance = generate_all_tables(filenames)

    with open(OUT_FOLDER + "risk_evaluation.tex", "w+") as risk_eval_file:
        risk_eval_file.write(txt)

    with open(OUT_FOLDER + "risk_acceptance.tex", "w+") as risk_acceptance_file:
        risk_acceptance_file.write(risk_acceptance)

if __name__ == '__main__':
    main()
